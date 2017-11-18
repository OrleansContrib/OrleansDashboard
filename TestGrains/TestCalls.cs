﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;

namespace TestGrains
{
    public static class TestCalls
    {
        public static Task Make(IClusterClient client, CancellationTokenSource tokenSource)
        {
            return Task.Run(async () =>
            {
                var messageGrain = client.GetGrain<ITestMessageBasedGrain>(42);

                await messageGrain.Receive("string");
                await messageGrain.ReceiveVoid(DateTime.UtcNow);
                await messageGrain.Notify(null);

                var genericGrain = client.GetGrain<ITestGenericGrain<string, int>>("test");

                await genericGrain.TestT("string");
                await genericGrain.TestU(1);
                await genericGrain.TestTU("string", 1);

                var random = new Random();

                while (!tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var testGrain = client.GetGrain<ITestGrain>(random.Next(500));

                        await testGrain.ExampleMethod1();
                        await testGrain.ExampleMethod2();

                        var genericClient = client.GetGrain<IGenericGrain<string>>("foo");

                        await genericClient.Echo("hello world");
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