﻿using System.Threading.Tasks;
using Orleans;
using OrleansDashboard;
using OrleansDashboard.Metrics;

namespace TestGrains
{
    public interface IGenericGrain<T> : IGrainWithStringKey
    {
        Task<T> Echo(T value);

        Task<T> EchoNoProfiling(T value);
    }

    public class GenericGrain<T> : Grain, IGenericGrain<T>
    {
        private readonly IGrainProfiler profiler;

        public GenericGrain(IGrainProfiler profiler)
        {
            this.profiler = profiler;
        }

        public Task<T> Echo(T value)
        {
            return Task.FromResult(value);
        }

        [NoProfiling]
        public async Task<T> EchoNoProfiling(T value)
        {
            await profiler.TrackAsync<GenericGrain<T>>(() => Task.Delay(1000));

            return value;
        }
    }
}