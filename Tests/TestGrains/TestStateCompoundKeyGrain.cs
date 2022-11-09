using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace TestGrains
{
    public interface ITestStateCompoundKeyGrain : IGrainWithIntegerCompoundKey
    {
        Task<InMemoryCounterState> GetState();
    }

    public class TestStateCompoundKeyGrain : Grain, ITestStateCompoundKeyGrain
    {
        private readonly Random _random = new Random();
        private readonly InMemoryCounterState _state;

        public TestStateCompoundKeyGrain()
        {
            _state = new InMemoryCounterState()
            {
                Counter = _random.Next(100),
                ActivatedDateTime = DateTime.UtcNow
            };
        }

        public Task<InMemoryCounterState> GetState()
        {
            return Task.FromResult(_state);
        }
    }
}