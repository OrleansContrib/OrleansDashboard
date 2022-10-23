using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace TestGrains
{
    public interface IFirstInteractionTestGrain : ITestGrain
    {
        Task CallFirstInteractionTestGrain(int id);

        Task CallSecondInteractionTestGrain(int id);
        
        Task CallThirdInteractionTestGrain(int id);
        
        Task CallTestGrain(int id);
    }
    
    public interface ISecondInteractionTestGrain : IFirstInteractionTestGrain
    {
    }
    
    public interface IThirdInteractionTestGrain : IFirstInteractionTestGrain
    {
    }

    public class FirstInteractionTestGrain : TestGrain, IFirstInteractionTestGrain
    {
        
        Random random = new Random();
        public Task CallFirstInteractionTestGrain(int id)
        {
            return GrainFactory.GetGrain<ISecondInteractionTestGrain>(id).CallSecondInteractionTestGrain(id);
        }

        public async Task CallSecondInteractionTestGrain(int id)
        {
            await GrainFactory.GetGrain<IThirdInteractionTestGrain>(id).ExampleMethod1();
            await GrainFactory.GetGrain<ITestGrain>(id).ExampleMethod1();
        }
        
        public Task CallThirdInteractionTestGrain(int id)
        {
            return GrainFactory.GetGrain<ITestGrain>(id).ExampleMethod1();
        }

        public Task CallTestGrain(int id)
        {
            return GrainFactory.GetGrain<ITestGrain>(id).ExampleMethod1();
        }
    }
    


    public class SecondInteractionTestGrain : FirstInteractionTestGrain, ISecondInteractionTestGrain
    {
       
    }
    
    public class ThirdInteractionTestGrain : FirstInteractionTestGrain, IThirdInteractionTestGrain
    {
       
    }
}