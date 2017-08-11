using Orleans;
using System;
using System.Threading.Tasks;

namespace TestGrains
{
    public interface ITestMessageBasedGrain : IGrainWithIntegerKey
    {
        Task<object> Receive(object message);
        Task ReceiveVoid(object message);
        Task Notify(object message);
    }

    public class TestMessageBasedGrain : Grain, ITestMessageBasedGrain
    {
        public Task<object> Receive(object message) => Task.FromResult((object)null);
        public Task ReceiveVoid(object message) => Task.CompletedTask;
        public Task Notify(object message) => Task.CompletedTask;
    }
}
