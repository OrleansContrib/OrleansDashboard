using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Model;

namespace OrleansDashboard.Implementation.Grains
{
    public class DashboardRemindersGrain : Grain, IDashboardRemindersGrain
    {
        private static readonly Immutable<ReminderResponse> EmptyReminders = new ReminderResponse
        {
            Reminders = Array.Empty<ReminderInfo>()
        }.AsImmutable();

        private readonly IReminderTable reminderTable;

        public DashboardRemindersGrain(IServiceProvider serviceProvider)
        {
            reminderTable = serviceProvider.GetService(typeof(IReminderTable)) as IReminderTable;
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            if (reminderTable == null)
            {
                return EmptyReminders;
            }

            var reminderData = await reminderTable.ReadRows(0, 0xffffffff);

            if(!reminderData.Reminders.Any())
            {
                return EmptyReminders;
            }

            return new ReminderResponse
            {
                Reminders = reminderData
                    .Reminders
                    .OrderBy(x => x.StartAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToReminderInfo)
                    .ToArray(),

                Count = reminderData.Reminders.Count
            }.AsImmutable();
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                PrimaryKey = entry.GrainId.Key.ToString(),
                GrainReference = entry.GrainId.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period,
            };
        }
    }
}