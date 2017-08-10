using Orleans;
using System;
using System.Threading.Tasks;

namespace TestGrains
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageArgumentAttribute : Attribute
    {}

    public interface ITestMessageBasedGrain : IGrainWithIntegerKey
    {
        Task Receive(object message);
    }

    public class TestMessageBasedGrain : Grain, ITestMessageBasedGrain
    {
        [MessageArgument]
        public Task Receive(object message)
        {
            return TaskDone.Done;
        }
    }
}
