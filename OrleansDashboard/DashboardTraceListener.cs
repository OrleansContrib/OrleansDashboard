using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OrleansDashboard
{
    public class DashboardTraceListener : TraceListener
    {

        List<Action<string>> actions;

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
                    this.Remove(action);
                }
                
            }
        }

        public void Add(Action<string> action)
        {
            this.actions.Add(action);
        }

        public void Remove(Action<string> action)
        {
            this.actions.Remove(action);
        }


        public override void WriteLine(string message) => this.Write(message + "\r\n");
    }
}
