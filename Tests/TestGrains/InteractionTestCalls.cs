using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;

namespace TestGrains
{
    public static class InteractionTestCalls
    {
        public static Task Make(IClusterClient client, CancellationTokenSource tokenSource)
        {
            return Task.Run(async () =>
            {
                var random = new Random();

                while (!tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        // client => First => Second
                        
                        // client => Second => Third
                        // client => Second => Test
                        
                        // client => Third => Test
                        var rnd = random.Next(1, 3);

                        switch (rnd)
                        {
                            case 1:
                            {
                                var testGrain = client.GetGrain<IFirstInteractionTestGrain>(random.Next(100));
                                await testGrain.CallFirstInteractionTestGrain(random.Next(100));
                                break;
                            }
                            case 2:
                            {
                                var testGrain = client.GetGrain<ISecondInteractionTestGrain>(random.Next(100));
                                await testGrain.CallSecondInteractionTestGrain(random.Next(100));
                                break;
                            }
                            case 3:
                            {
                                var testGrain = client.GetGrain<IThirdInteractionTestGrain>(random.Next(100));
                                await testGrain.CallThirdInteractionTestGrain(random.Next(100));
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // Grain might throw exception to test error rate.
                    }
                }
            });
        }
    }
}