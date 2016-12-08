using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGrains
{
    public interface IGenericGrain<T> : IGrainWithStringKey
    {
        Task<T> Echo(T value);
    }

    public class GenericGrain<T> : Grain, IGenericGrain<T>
    {
        public Task<T> Echo(T value)
        {
            return Task.FromResult(value);
        }
    }
}
