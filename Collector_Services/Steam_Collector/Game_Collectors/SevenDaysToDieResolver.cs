using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData._7DaysToDie;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class SevenDaysToDieResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public SevenDaysToDieResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers, IClickHouseService clickHouse, ILogger<SevenDaysToDieResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "7 Days To Die";
    public override ulong AppId => 251570;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.SevenDaysToDieServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var sevenDTDServers = new List<SevenDaysToDieServer>(servers.Select(ResolveServerDetails));
        await Submit(sevenDTDServers);
    }

    private SevenDaysToDieServer ResolveServerDetails(IGenericServerInfo server)
    {
        var customServer = new SevenDaysToDieServer();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}