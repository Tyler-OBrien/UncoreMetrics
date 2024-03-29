﻿using System.Net;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.ClickHouse.Models;
using UncoreMetrics.Data.Discord;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;
using UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

/// <summary>
///     Base Resolver for all of the various Server Resolvers.
/// </summary>
public abstract class BaseResolver
{
    internal readonly IClickHouseService _clickHouseService;
    internal readonly SteamCollectorConfiguration _configuration;
    internal readonly ServersContext _genericServersContext;
    internal readonly ILogger _logger;
    internal readonly ISteamServers _steamServers;
    internal readonly IServerUpdateQueue _serverUpdateQueue;


    public BaseResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger logger, IServerUpdateQueue serverUpdateQueue)
    {
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
        _steamServers = steamServers;
        _clickHouseService = clickHouse;
        _logger = logger;
        _serverUpdateQueue = serverUpdateQueue;
    }

    /// <summary>
    ///     Name of Resolver
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Steam App Id (Ulong/64 bit unsigned number)
    /// </summary>
    public abstract ulong AppId { get; }

    /// <summary>
    ///     Allow overriding of Query for Steam Server List (Discovery). By default, it grabs all dedicated, non-empty servers
    ///     matching the <see cref="AppId" />
    /// </summary>
    public virtual SteamServerListQueryBuilder? CustomQuery { get; }


    /// <summary>
    /// If failing to get A2S_Players should fail the check and not update anything (useful for Rust, where max players is often above what a2s_info is useful for..
    /// </summary>
    public virtual bool A2SPlayersFailEverything => false;



    /// <summary>
    ///     Can be overriden to handle the behavior of the result of Polls. By default, it calls HandleServersGenericStatusChange, updates each Server with the latest
    ///     information, and calls HandleServersGeneric
    /// </summary>
    /// <param name="servers"></param>
    /// <returns></returns>
    public virtual async Task PollResult(List<PollServerInfo> servers)
    {
        await HandleServersGenericStatusChange(servers.ToList<IGenericServerInfo>(), false);
        servers.ForEach(server => server.UpdateServer(AppId, _configuration.SecondsBetweenChecks,
            _configuration.SecondsBetweenFailedChecks, _configuration.DaysUntilServerMarkedDead, A2SPlayersFailEverything));
        await HandleServersGeneric(servers.ToList<IGenericServerInfo>());
    }

    /// <summary>
    ///     Can be overriden to handle the behavior of the result of Discovery. By default, it calls HandleServersGenericStatusChange, updates each Server with the
    ///     latest information, and calls HandleServersGeneric
    /// </summary>
    /// <param name="servers"></param>
    /// <returns></returns>
    public virtual async Task DiscoveryResult(List<DiscoveredServerInfo> servers)
    {
        await HandleServersGenericStatusChange(servers.ToList<IGenericServerInfo>(), true);
        servers.ForEach(server => server.UpdateServer(_configuration.SecondsBetweenChecks));
        await HandleServersGeneric(servers.ToList<IGenericServerInfo>());
    }

    /// <summary>
    /// Can be overriden to have custom behavior for server status. Called by Discovery/Poll Result to handle status changes, forwarded to NATS.
    /// </summary>
    /// <param name="servers"></param>
    /// <param name="isDiscovery">If this run is discovery, don't mark any servers are newly down</param>
    /// <returns></returns>
    public virtual async Task HandleServersGenericStatusChange(List<IGenericServerInfo> servers, bool isDiscovery)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(_configuration.NATSConnectionURL))
            {
                _logger.LogInformation("Not pushing updates to NATS Since Disabled in Config");
                return;
            }

            List<Guid> newlyDownServers = new List<Guid>();
            List<Guid> newlyUpServers = new List<Guid>();
            foreach (var genericServerInfo in servers)
            {
                if (genericServerInfo.ExistingServer != null)
                {
                    if (genericServerInfo.ServerInfo == null && genericServerInfo.ExistingServer.IsOnline && isDiscovery == false)
                        newlyDownServers.Add(genericServerInfo.ExistingServer.ServerID);
                    else if (genericServerInfo.ServerInfo != null && genericServerInfo.ExistingServer.IsOnline == false)
                        newlyUpServers.Add(genericServerInfo.ExistingServer.ServerID);
                }
            }

            await _serverUpdateQueue.ServerUpdate(new ServerUpdateNATs()
                { ServersDown = newlyDownServers, ServersUp = newlyUpServers });
            _logger.LogInformation($"Pushed {newlyDownServers.Count} Down Servers and {newlyUpServers.Count} Up server status to NATS.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleServersGenericStatusChange, non-fatal, contuining.");
        }
    }


    /// <summary>
    ///     This method is implemented by Game Resolvers. It takes in a list with generic server details, and must handle any
    ///     modifications and saving it wants to do.
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public abstract Task HandleServersGeneric(List<IGenericServerInfo> server);

    /// <summary>
    ///     Used in polling, allows Game Resolvers to specify their own logic for retrieving a list of servers to poll. Allows
    ///     filtering/limiting/etc.
    /// </summary>
    /// <returns></returns>
    public abstract Task<List<Server>> GetServers();

    /// <summary>
    ///     Handles polling. By default, it calls GenericServerPoll with the result of GetServers, and then calls PollResult
    ///     with the results. Can be overriden with custom logic.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> Poll()
    {
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_configuration.RunForceTimeoutMinutes));
        var servers = await _steamServers.GenericServerPoll(await GetServers(), cancellationTokenSource.Token);
        // Abort run if no servers are returned..
        if (servers.Count == 0)
            return 0;
        await PollResult(servers);
        return servers.Count;
    }

    /// <summary>
    ///     Handles Discovery. By default, it grabs the Steam Server List Query or makes a default one with AppId and filtering
    ///     for Dedicated, non-empty servers, uses GenericServerDiscovery and calls DiscoveryResults. Can be overriden.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> Discovery()
    {
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_configuration.RunForceTimeoutMinutes));
        var query = CustomQuery;
        if (query == null) query = SteamServerListQueryBuilder.New().AppID(AppId.ToString()).Dedicated().NotEmpty();

        var servers = await _steamServers.GenericServerDiscovery(query, cancellationTokenSource.Token);
        var addresses = servers.Select(i => i.Address).ToList();
        var ports = servers.Select(i => i.Port).ToList();

        var entities = await _genericServersContext.Servers
            .Where(a => addresses.Contains(a.Address) || ports.Contains(a.QueryPort)).AsNoTracking()
            .Select(i => new { i.Address, i.QueryPort, i.ServerID, i.IsOnline }).ToListAsync(cancellationTokenSource.Token); // SQL IN

        var hashMapDictionary = new Dictionary<IPEndPoint, Tuple<Guid, bool>>();
        foreach (var server in entities)
            hashMapDictionary.Add(new IPEndPoint(server.Address, server.QueryPort), new Tuple<Guid, bool>(server.ServerID, server.IsOnline));

        servers.ForEach(server =>
        {
            if (hashMapDictionary.TryGetValue(new IPEndPoint(server.Address, server.Port), out var value))
            {
                if (server.ExistingServer == null) server.ExistingServer = new Server();
                server.ExistingServer.ServerID = value.Item1;
                server.ExistingServer.IsOnline = value.Item2;
            }
        });
        await DiscoveryResult(servers);
        return servers.Count;
    }


    // This is super messy, and should be cleaned up at some point.
    // This is necessary due to us needing basically two primary keys
    // One being the GUID and { IPAddressBytes, Port}
    // Npsql ef core doesn't let us have the GUID just be a unique index on the Servers table, and be the foreign key of the individual server tables.
    // So for now GUID is the primary key of all tables, and { IpAddressBytes, Port} is just a unique constraint on the main table. We could do an upsert to meet that constraint, but BulkExtensions doesn't support it.
    // So our hacky workaround is manually looking for any existing Servers in a transaction, and attaching the found GUID if the server already exists, which is just needed for Discovery, not Polling.
    public async Task Submit<T>(List<T> servers) where T : Server, new()
    {
        using var transaction = await _genericServersContext.Database.BeginTransactionAsync();

        if (servers.Any(server => server.ServerID == Guid.Empty))
        {
            var unfoundServers = servers.Where(server => server.ServerID == Guid.Empty);

            var addresses = unfoundServers.Select(i => i.Address).ToList();
            var ports = unfoundServers.Select(i => i.QueryPort).ToList();

            var entities = await _genericServersContext.Servers
                .Where(a => addresses.Contains(a.Address) || ports.Contains(a.QueryPort)).AsNoTracking()
                .Select(i => new { i.Address, i.QueryPort, i.ServerID }).ToListAsync(); // SQL IN

            var hashMapDictionary = new Dictionary<IPEndPoint, Guid>();
            foreach (var server in entities)
                hashMapDictionary.Add(new IPEndPoint(server.Address, server.QueryPort), server.ServerID);

            foreach (var server in servers)
                if (hashMapDictionary.TryGetValue(new IPEndPoint(server.Address, server.QueryPort), out var value))
                    server.ServerID = value;

#if DEBUG
            _logger.LogDebug(
                "Found {serversFoundUUIDsCount} UUIDs Non-New Servers, out of {serversCount}.",
                servers.Count(server => server.ServerID != Guid.Empty), servers.Count);
#endif
        }

        foreach (var server in servers)
            if (server.ServerID == Guid.Empty)
                server.ServerID = Guid.NewGuid();
        var serverList = servers.ToList<Server>();
        // Update Generic Servers
        await InsertGenericServer(serverList);
        // Update Server Specific Info
        await _genericServersContext.BulkInsertOrUpdateAsync(servers);
        await transaction.CommitAsync();
        await ClickHouseSubmit(serverList);
    }

    private async Task ClickHouseSubmit(List<Server> servers)
    {
        await _clickHouseService.Insert(servers.Select(ClickHouseGenericServer.FromServer));
    }

    private async Task InsertGenericServer(List<Server> servers)
    {
        var bulkConfig = new BulkConfig
        {
            PropertiesToExclude = new List<string> { "SearchVector" },
            PropertiesToExcludeOnUpdate = new List<string> { "FoundAt", "ServerID", "SearchVector" },
        };
        await _genericServersContext.BulkInsertOrUpdateAsync(servers, bulkConfig);
    }
}