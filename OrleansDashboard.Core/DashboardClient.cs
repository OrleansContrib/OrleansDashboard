using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using OrleansDashboard.Model;
using OrleansDashboard.Model.History;

namespace OrleansDashboard
{
    public class DashboardClient : IDashboardClient
    {
        private readonly IDashboardGrain dashboardGrain;
        private readonly IDashboardRemindersGrain remindersGrain;
        private readonly IGrainFactory grainFactory;

        public DashboardClient(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
            dashboardGrain = grainFactory.GetGrain<IDashboardGrain>(0);
            remindersGrain = grainFactory.GetGrain<IDashboardRemindersGrain>(0);
        }

        public async Task<Immutable<DashboardCounters>> DashboardCounters()
        {
            return await dashboardGrain.GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> ClusterStats()
        {
            return await dashboardGrain.GetClusterTracing().ConfigureAwait(false);
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            return await remindersGrain.GetReminders(pageNumber, pageSize).ConfigureAwait(false);
        }

        public async Task<Immutable<SiloRuntimeStatistics[]>> HistoricalStats(string siloAddress)
        {
            return await Silo(siloAddress).GetRuntimeStatistics().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, string>>> SiloProperties(string siloAddress)
        {
            return await Silo(siloAddress).GetExtendedProperties().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainTraceEntry>>> SiloStats(string siloAddress)
        {
            return await dashboardGrain.GetSiloTracing(siloAddress).ConfigureAwait(false);
        }

        public async Task<Immutable<StatCounter[]>> GetCounters(string siloAddress)
        {
            return await Silo(siloAddress).GetCounters().ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, Dictionary<string, GrainTraceEntry>>>> GrainStats(string grainName)
        {
            return await dashboardGrain.GetGrainTracing(grainName).ConfigureAwait(false);
        }

        public async Task<Immutable<Dictionary<string, GrainMethodAggregate[]>>> TopGrainMethods(int take)
        {
            return await dashboardGrain.TopGrainMethods(take).ConfigureAwait(false);
        }

        private ISiloGrain Silo(string siloAddress)
        {
            return grainFactory.GetGrain<ISiloGrain>(siloAddress);
        }

        public async Task<ExpandoObject> GetGrainState(string id, string grainType)
        {
            var result = new ExpandoObject();
            try
            {
                var implementationType = AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(s => s.GetTypes())
                                    .Where(w => w.Name.Equals(grainType))
                                    .FirstOrDefault();

                var impProperties = implementationType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                var impFields = implementationType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);


                var filterProps = impProperties
                                    .Where(w => w.PropertyType.IsAssignableTo(typeof(IStorage)))
                                    .Select(s => s.PropertyType.GetGenericArguments().First());

                var filterFields = impFields
                                    .Where(w => w.FieldType.IsAssignableTo(typeof(IStorage)))
                                    .Select(s => s.FieldType.GetGenericArguments().First());


                var interfaceTypes = implementationType.GetInterfaces();

                foreach (var interfaceType in interfaceTypes)
                {
                    try
                    {
                        var grain = grainFactory.GetGrain(interfaceType, id);

                        var methods = interfaceType.GetMethods()
                            .Where(w => w.GetParameters().Length == 0
                            );

                        foreach (var method in methods)
                        {
                            try
                            {
                                if (method.ReturnType.IsAssignableTo(typeof(Task)) 
                                    &&
                                    (
                                        method.ReturnType.GetGenericArguments()
                                                    .Any(a => filterProps.Any(f => f == a))
                                        ||
                                        method.ReturnType.GetGenericArguments()
                                                    .Any(a => filterFields.Any(f => f == a))
                                    )
                                )
                                {
                                    var task = (method.Invoke(grain, null) as Task);
                                    var resultProperty = task.GetType().GetProperty("Result");

                                    if (resultProperty == null)
                                        continue;

                                    await task.ConfigureAwait(false);

                                    result.TryAdd(method.Name, resultProperty.GetValue(task));
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception)
            {

            }
            

            return result;
        }
    }
}
