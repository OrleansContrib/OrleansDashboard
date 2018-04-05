using System;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public interface IExternalDispatcher
    {
        bool CanDispatch();

        Task DispatchAsync(Func<Task> action);

        Task<T> DispatchAsync<T>(Func<Task<T>> action);
    }
}