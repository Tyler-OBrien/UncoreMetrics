using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers.Maxmind;
using Steam_Collector.Models;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public partial class SteamServers : ISteamServers
{
    private readonly BaseConfiguration _configuration;
    private readonly ServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ISteamAPI _steamApi;


    public SteamServers(ISteamAPI steamAPI, IGeoIPService geoIPService,
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
    }
}