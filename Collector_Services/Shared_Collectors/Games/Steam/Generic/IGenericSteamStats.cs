using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public partial interface IGenericSteamStats
{
    public Task BulkInsertOrUpdate<T>(List<T> servers) where T : Server, new();
}