using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public partial interface ISteamServers
{
    public Task BulkInsertOrUpdate<T>(List<T> servers) where T : Server, new();
}