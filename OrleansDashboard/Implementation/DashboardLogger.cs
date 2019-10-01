using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0069 // Disposable fields should be disposed

namespace OrleansDashboard.Implementation
{
    public class DashboardLogger : ILoggerProvider, ILogger
    {
        private readonly NoopDisposable scope = new NoopDisposable();
        private readonly List<Action<string>> actions;

        public static readonly DashboardLogger Instance = new DashboardLogger();

        private DashboardLogger()
        {
            actions = new List<Action<string>>();
        }

        public void Add(Action<string> action)
        {
            actions.Add(action);
        }

        public void Remove(Action<string> action)
        {
            actions.Remove(action);
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
            if (actions.Count <= 0) return;

            var logBuilder = new StringBuilder();

            logBuilder.Append(DateTime.UtcNow);
            logBuilder.Append(" ");
            logBuilder.Append(GetLogLevelString(logLevel));
            logBuilder.Append(": [");
            logBuilder.Append(eventId.ToString().PadLeft(8));
            logBuilder.Append("] ");
            logBuilder.Append(formatter(state, exception));

            var message = logBuilder.ToString();

            foreach (var action in actions)
            {
                action(message);
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

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
