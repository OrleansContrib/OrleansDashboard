using Orleans;
using System;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace TestGrains
{

    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task ExampleMethod1();
        Task ExampleMethod2();
    }

    public class TestGrain : Grain, ITestGrain, IRemindable
    {
        Random rand = new Random();

        public async Task ExampleMethod1()
        {
            await Task.Delay(rand.Next(100));
        }

        public Task ExampleMethod2()
        {

            if (rand.Next(100) > 50) throw new ApplicationException();
            return TaskDone.Done;
        }

        public override async Task OnActivateAsync()
        {
            await RegisterOrUpdateReminder("Frequent", TimeSpan.Zero, TimeSpan.FromMinutes(1));
            await RegisterOrUpdateReminder("Daily", TimeSpan.Zero, TimeSpan.FromDays(1));
            await RegisterOrUpdateReminder("Weekly", TimeSpan.Zero, TimeSpan.FromDays(7));
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskDone.Done;
        }
    }
}
