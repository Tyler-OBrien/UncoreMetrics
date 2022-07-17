using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using Shared_Collectors.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public partial class GenericSteamStats : IGenericSteamStats
{
    private readonly GenericServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ISteamAPI _steamApi;
    private readonly BaseConfiguration _configuration;


    public GenericSteamStats(ISteamAPI steamAPI, IGeoIPService geoIPService, IOptions<BaseConfiguration> baseConfiguration, GenericServersContext serversContext)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;

    }






}