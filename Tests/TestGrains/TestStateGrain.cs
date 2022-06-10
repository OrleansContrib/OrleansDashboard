using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace TestGrains
{
    public interface ITestStateGrain : IGrainWithIntegerKey
    {

        Task<CounterState> GetState();
    }

    public class TestStateGrain : Grain, ITestStateGrain
    {
        private readonly Random random = new Random();

        private readonly IPersistentState<CounterState> _counter;

        public TestStateGrain(
            [PersistentState("counter")]IPersistentState <CounterState> counter)
        {
            _counter = counter;
        }

        public async Task<CounterState> GetState()
        {
            _counter.State.Counter = random.Next(100);
            _counter.State.CurrentDateTime = DateTime.UtcNow;
            await _counter.WriteStateAsync();
            return _counter.State;
        }
    }

    [GenerateSerializer]
    public class CounterState
    {
        [Id(0)]
        public int Counter { get; set; }
        [Id(1)]
        public DateTime CurrentDateTime { get; set; }
    }
}