using System;
using System.Threading;
using System.Threading.Tasks;
using OrleansDashboard.Client;

namespace OrleansDashboard.Dispatchers
{
    public class SiloDispatcher : IExternalDispatcher, IDisposable
    {
        private TaskScheduler _scheduler;

        public Task DispatchAsync(Func<Task> action)
        {
            if (_scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _scheduler).Unwrap();
        }

        public Task<T> DispatchAsync<T>(Func<Task<T>> action)
        {
            if (_scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _scheduler).Unwrap();
        }

        public bool CanDispatch()
        {
            return _scheduler != null;
        }

        public void Setup()
        {
            _scheduler = TaskScheduler.Current;
        }

        public void Dispose()
        {
            _scheduler = null;
        }
    }
}