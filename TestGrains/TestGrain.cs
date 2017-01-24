using Orleans;
using System;
using System.Threading.Tasks;

namespace TestGrains
{

    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task ExampleMethod1();
        Task ExampleMethod2();
    }

    public class TestGrain : Grain, ITestGrain
    {
        Random rand = new Random();

        public async Task ExampleMethod1()
        {
            await Task.Delay(rand.Next(100));
        }

        public Task ExampleMethod2()
        {

            if (rand.Next(100) > 50) throw new Exception();
            return TaskDone.Done;
        }
    }
}
