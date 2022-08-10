using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Helpers.Maxmind;
using UncoreMetrics.Steam_Collector.Helpers.ScrapeJobStatus;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

namespace UncoreMetrics.Steam_Collector.SteamServers;

public partial class SteamServers : ISteamServers
{
    private readonly SteamCollectorConfiguration _configuration;
    private readonly ServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ILogger _logger;
    private readonly IScrapeJobStatusService _scrapeJobStatusService;
    private readonly ISteamAPI _steamApi;


    public SteamServers(ISteamAPI steamAPI, IGeoIPService geoIPService, IScrapeJobStatusService scrapeJobStatusService,
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ILogger<SteamServers> logger)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
        _logger = logger;
        _scrapeJobStatusService = scrapeJobStatusService;
    }
}