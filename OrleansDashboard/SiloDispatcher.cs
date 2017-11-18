using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class SiloDispatcher : IExternalDispatcher
    {
        private static TaskScheduler scheduler;

        public Task DispatchAsync(Func<Task> action)
        {
            return Task<Task>.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
        }

        public Task<T> DispatchAsync<T>(Func<Task<T>> action)
        {
            return Task<Task<T>>.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
        }

        public bool CanDispatch()
        {
            return scheduler != null;
        }

        public static void Setup()
        {
            scheduler = TaskScheduler.Current;
        }
    }
}