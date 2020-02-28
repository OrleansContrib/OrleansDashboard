using System;

namespace OrleansDashboard
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class NoProfilingAttribute : Attribute
    {
    }
}
