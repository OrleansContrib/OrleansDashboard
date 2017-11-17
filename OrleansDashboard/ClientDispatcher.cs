using System;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public sealed class ClientDispatcher : IExternalDispatcher
    {
        public bool CanDispatch()
        {
            return true;
        }

        public Task DispatchAsync(Func<Task> action)
        {
            return action();
        }

        public Task<T> DispatchAsync<T>(Func<Task<T>> action)
        {
            return action();
        }
    }
}