using System.Threading.Tasks;
using Orleans;

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