using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class SiloDispatcher : IExternalDispatcher, IDisposable
    {
        private TaskScheduler scheduler;

        public Task DispatchAsync(Func<Task> action)
        {
            if (scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
        }

        public Task<T> DispatchAsync<T>(Func<Task<T>> action)
        {
            if (scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
        }

        public bool CanDispatch()
        {
            return scheduler != null;
        }

        public void Setup()
        {
            scheduler = TaskScheduler.Current;
        }

        public void Dispose()
        {
            scheduler = null;
        }
    }
}