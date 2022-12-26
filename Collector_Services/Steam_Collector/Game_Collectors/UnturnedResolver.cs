using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Unturned;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class UnturnedResolver : BaseResolver
{

    public UnturnedResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<UnturnedResolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
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
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Abort run if less then 1000 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 1000 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
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