using System;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0069 // Disposable fields should be disposed

namespace OrleansDashboard
{
    public class DashboardLogger : ILoggerProvider, ILogger
    {
        private readonly NoopDisposable scope = new NoopDisposable();
        private ImmutableArray<Action<EventId, LogLevel, string>> actions;

        public static readonly DashboardLogger Instance = new DashboardLogger();

        private DashboardLogger()
        {
            actions = ImmutableArray<Action<EventId, LogLevel, string>>.Empty;
        }

        public void Add(Action<EventId, LogLevel, string> action)
        {
            actions = actions.Add(action);
        }

        public void Remove(Action<EventId, LogLevel, string> action)
        {
            actions = actions.Remove(action);
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var currentActions = actions;

            if (currentActions.Length <= 0)
            {
                return;
            }

            var logMessage = formatter(state, exception);

            foreach (var action in currentActions)
            {
                action(eventId, logLevel, logMessage);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return scope;
        }

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
