using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.Arma3;

namespace Steam_Collector.Game_Collectors;

public class Arma3Resolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public Arma3Resolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Arma3";
    public override ulong AppId => 107410;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<Arma3Server>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.Arma3Servers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    private Arma3Server ResolveServerDetails(IGenericServerInfo server)
    {
        Arma3Server customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}