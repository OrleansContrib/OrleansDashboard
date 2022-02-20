namespace OrleansDashboard.Metrics.History
{
    public sealed record HistoryKey(string SiloAddress, string Grain, string Method);
}
