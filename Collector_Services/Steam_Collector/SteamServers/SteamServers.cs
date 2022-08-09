using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers.Maxmind;
using Steam_Collector.Helpers.ScrapeJobStatus;
using Steam_Collector.Models;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public partial class SteamServers : ISteamServers
{
    private readonly SteamCollectorConfiguration _configuration;
    private readonly ServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ISteamAPI _steamApi;
    private readonly ILogger _logger;
    private readonly IScrapeJobStatusService _scrapeJobStatusService;


    public SteamServers(ISteamAPI steamAPI, IGeoIPService geoIPService, IScrapeJobStatusService scrapeJobStatusService,
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ILogger<SteamServers> logger)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
        _logger = logger;
        _scrapeJobStatusService = scrapeJobStatusService;
    }
}