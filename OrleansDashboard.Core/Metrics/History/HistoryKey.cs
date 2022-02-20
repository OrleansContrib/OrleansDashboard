namespace OrleansDashboard.Metrics.History
{
    public record struct HistoryKey(string SiloAddress, string Grain, string Method);
}
