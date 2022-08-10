using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Unturned;

namespace Steam_Collector.Game_Collectors;

public class UnturnedResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public UnturnedResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers, IClickHouseService clickHouse, ILogger<UnturnedResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Unturned";
    public override ulong AppId => 304930;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<UnturnedServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.UnturnedServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    private UnturnedServer ResolveServerDetails(IGenericServerInfo server)
    {
        UnturnedServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null)
        {
            customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
            //Slightly Messy for now..
            var messageDetails = server.ServerRules.TryGetRunningList("Custom_Link_Message_{0}");
            var messageLinks = server.ServerRules.TryGetRunningList("Custom_Link_Url_{0}");
            if (messageLinks.Count == messageDetails.Count && messageLinks.Count != 0)
            {
                customServer.CustomLinks = new List<string>(messageLinks.Count);
                for (var i = 0; i < messageDetails.Count; i++)
                {
                    var details = messageDetails[i];
                    var link = messageLinks[i];
                    customServer.CustomLinks.Add($"{details}::{link}");
                }
            }
        }

        return customServer;
    }
}