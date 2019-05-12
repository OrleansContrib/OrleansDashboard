using System;

namespace OrleansDashboard.Metrics
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class NoProfilingAttribute : Attribute
    {
    }
}
