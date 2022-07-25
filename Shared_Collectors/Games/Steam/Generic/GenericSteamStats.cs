using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models;
using Shared_Collectors.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public partial class GenericSteamStats : IGenericSteamStats
{
    private readonly BaseConfiguration _configuration;
    private readonly ServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ISteamAPI _steamApi;


    public GenericSteamStats(ISteamAPI steamAPI, IGeoIPService geoIPService,
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
    }


    public async Task BulkInsertOrUpdate<T>(List<T> servers) where T : GenericServer, new()
    {
        await InsertGenericServer(servers.ToList<GenericServer>());
        await _genericServersContext.BulkInsertOrUpdateAsync(servers);
    }

    private async Task InsertGenericServer(List<GenericServer> servers)
    {
        var bulkConfig = new BulkConfig
        {
            PropertiesToExclude = new List<string> { "SearchVector" },
            PropertiesToExcludeOnUpdate = new List<string> { "FoundAt", "ServerID", "SearchVector" },
            UseTempDB = false
        };


        await _genericServersContext.BulkInsertOrUpdateAsync(servers, bulkConfig);
    }
}