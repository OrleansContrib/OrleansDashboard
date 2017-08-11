using System.Reflection;

using Orleans;
using Orleans.CodeGeneration;

namespace OrleansDashboard
{
    public interface IGrainMethodNameFormatter
    {
        string Format(MethodInfo targetMethod, InvokeMethodRequest request, IGrain grain);
    }

    class DefaultGrainMethodNameFormatter : IGrainMethodNameFormatter
    {
        public string Format(MethodInfo targetMethod, InvokeMethodRequest request, IGrain grain) => targetMethod?.Name ?? "Unknown";
    }
}