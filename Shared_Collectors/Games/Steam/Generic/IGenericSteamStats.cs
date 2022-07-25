using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public partial interface IGenericSteamStats
{
    public Task BulkInsertOrUpdate<T>(List<T> servers) where T : GenericServer, new();
}