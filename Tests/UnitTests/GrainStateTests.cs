using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using OrleansDashboard;
using Orleans.TestingHost;
using TestGrains;
using Orleans.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Newtonsoft.Json.Linq;
using OrleansDashboard.Implementation.Helpers;

namespace UnitTests
{
  public class GrainStateTests : IDisposable
  {
    private readonly TestCluster _cluster;
    public GrainStateTests()
    {
      var builder = new TestClusterBuilder();
      builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
      _cluster = builder.Build();
      _cluster.Deploy();
    }

    public void Dispose()
    {
      _cluster.StopAllSilos();
    }

    [Fact]
    public void TestGetGrainsTypes()
    {
      var types = GrainStateHelper.GetGrainTypes()
                  .Select(s => s.Namespace + "." + s.Name);

      Assert.Contains("TestGrains.TestStateInMemoryGrain", types);
    }

    [Fact]
    public async void TestWithGetStateMethod()
    {
      var dashboardGrain = _cluster.GrainFactory.GetGrain<IDashboardGrain>(1);
      var stateGrain = _cluster.GrainFactory.GetGrain<ITestStateInMemoryGrain>(123);

      var immutableState = await dashboardGrain.GetGrainState("123", "TestGrains.TestStateInMemoryGrain");

      dynamic state = JObject.Parse(immutableState.Value);

      var stateFromGrain = await stateGrain.GetState();
      int counter = state.GetState.Counter;
      Assert.Equal(stateFromGrain.Counter, counter);
    }

    [Fact]
    public async void TestWithIStorageField()
    {
      var dashboardGrain = _cluster.GrainFactory.GetGrain<IDashboardGrain>(1);
      var stateGrain = _cluster.GrainFactory.GetGrain<ITestStateGrain>(123);
      await stateGrain.WriteCounterState(new CounterState
      {
        Counter = 5,
        CurrentDateTime = DateTime.UtcNow
      });
      var immutableState = await dashboardGrain.GetGrainState("123", "TestGrains.TestStateGrain");

      dynamic state = JObject.Parse(immutableState.Value);

      var stateFromGrain = await stateGrain.GetCounterState();
      int counter = state.GetCounterState.Counter;
      Assert.Equal(stateFromGrain.Counter, counter);
    }


    public class TestSiloConfigurations : ISiloConfigurator
    {
      public void Configure(ISiloBuilder siloBuilder)
      {


        siloBuilder.Services.AddOrleans((builder) =>
        {
          builder.UseInMemoryReminderService();
          builder.AddMemoryGrainStorageAsDefault();


          builder.UseDashboard(options =>
                  {
                    options.HostSelf = true;
                  });
        });
      }
    }

    [Fact]
    public void TestGetCallableGrains()
    {
        var grains = GrainStateHelper.GetCallableGrainTypes().Select(x => x.FullName).ToList();
        Assert.Equal(5, grains.Count());
        Assert.True(grains.Contains("TestGrains.ITestGrain"));
    }

    [Fact]
    public void TestGetCallableGrainMethods()
    {
        var methods = GrainStateHelper.GetCallableGrainMethods().ToList();
        Assert.Equal(5, methods.Count());
        foreach (var method in methods)
        {
            Console.WriteLine(method.DeclaringType.FullName + "." + method.Name);
        }
    }

  }
}
