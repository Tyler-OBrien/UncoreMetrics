using Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public partial interface ISteamServers
{
    public Task<List<PollServerInfo<T>>> GenericServerPoll<T>(ulong appID) where T : Server, new();
}