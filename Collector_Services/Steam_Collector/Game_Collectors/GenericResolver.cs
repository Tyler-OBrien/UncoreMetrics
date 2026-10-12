using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

/// <summary>
///     Resolver for any game in <see cref="GenericGames" />. Servers live only in the shared Servers table, and
///     A2S_RULES isn't queried since nothing game-specific is stored.
/// </summary>
public class GenericResolver : BaseResolver
{
    private readonly GenericGame _game;

    public GenericResolver(GenericGame game,
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<GenericResolver> logger,
        IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
        _game = game;
    }

    public override string Name => _game.Name;
    public override ulong AppId => _game.AppId;
    public override bool QueryRules => false;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.Servers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Batch polls: wait for the game's batch size of due servers, unless one has already waited longer than MaxPollDelay
        if (!ShouldPoll(servers, _game.PollBatchSize, DateTime.UtcNow))
            return new List<Server>();
        return servers;
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        await Submit(servers.Select(ResolveServerDetails).ToList());
    }

    private Server ResolveServerDetails(IGenericServerInfo server)
    {
        var resolved = new Server();
        if (server.ExistingServer != null)
            resolved.Copy(server.ExistingServer);
        resolved.Game = Name;
        return resolved;
    }
}
