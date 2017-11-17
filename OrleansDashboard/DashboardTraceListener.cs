using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OrleansDashboard
{
    public class DashboardTraceListener : TraceListener
    {
        private readonly List<Action<string>> actions;

        public DashboardTraceListener()
        {
            actions = new List<Action<string>>();
        }

        public override void Write(string message)
        {
            foreach (var action in actions.ToArray())
            {
                try
                {
                    action(message);
                }
                catch (Exception)
                {
                    Remove(action);
                }
            }
        }

        public void Add(Action<string> action)
        {
            actions.Add(action);
        }

        public void Remove(Action<string> action)
        {
            actions.Remove(action);
        }

        public override void WriteLine(string message) => Write(message + "\r\n");
    }
}
