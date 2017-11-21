using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class SiloDispatcher : IExternalDispatcher
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static TaskScheduler scheduler;

        public Task DispatchAsync(Func<Task> action)
        {
            if (scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task<Task>.Factory.StartNew(action, cts.Token, TaskCreationOptions.None, scheduler).Unwrap();
        }

        public Task<T> DispatchAsync<T>(Func<Task<T>> action)
        {
            if (scheduler == null)
            {
                throw new InvalidOperationException("The dispatcher has already been closed.");
            }

            return Task<Task<T>>.Factory.StartNew(action, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
        }

        public bool CanDispatch()
        {
            return scheduler != null;
        }

        public static void Setup()
        {
            scheduler = TaskScheduler.Current;
        }

        public static void Teardown()
        {
            cts.Cancel();

            scheduler = null;
        }
    }
}