using System.Net;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.Game_Collectors;

/// <summary>
/// Base Resolver for all of the various Server Resolvers.
/// </summary>
public abstract class BaseResolver
{
    private readonly BaseConfiguration _configuration;
    private readonly ServersContext _genericServersContext;
    private readonly ISteamServers _steamServers;


    public BaseResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers)
    {
        _genericServersContext = serversContext;
        _configuration = baseConfiguration.Value;
        _steamServers = steamServers;
    }

    /// <summary>
    /// Name of Resolver
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Steam App Id (Ulong/64 bit unsigned number)
    /// </summary>
    public abstract ulong AppId { get; }

    /// <summary>
    /// Allow overriding of Query for Steam Server List (Discovery). By default, it grabs all dedicated, non-empty servers matching the <see cref="AppId"/>
    /// </summary>
    public virtual SteamServerListQueryBuilder? CustomQuery { get; }

    /// <summary>
    /// Can be overriden to handle the behavior of the result of Polls. By default, it updates each Server with the latest information, and calls HandleServersGeneric
    /// </summary>
    /// <param name="servers"></param>
    /// <returns></returns>
    public virtual async Task PollResult(List<PollServerInfo> servers)
    {
        servers.ForEach(server => server.UpdateServer(_configuration.SecondsBetweenChecks,
            _configuration.SecondsBetweenFailedChecks, _configuration.DaysUntilServerMarkedDead));
        await HandleServersGeneric(servers.ToList<IGenericServerInfo>());
    }

    /// <summary>
    /// Can be overriden to handle the behavior of the result of Discovery. By default, it updates each Server with the latest information, and calls HandleServersGeneric
    /// </summary>
    /// <param name="servers"></param>
    /// <returns></returns>
    public virtual async Task DiscoveryResult(List<DiscoveredServerInfo> servers)
    {
        servers.ForEach(server => server.UpdateServer(_configuration.SecondsBetweenChecks));
        await HandleServersGeneric(servers.ToList<IGenericServerInfo>());
    }

    /// <summary>
    ///     This method is implemented by Game Resolvers. It takes in a list with generic server details, and must handle any
    ///     modifications and saving it wants to do.
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public abstract Task HandleServersGeneric(List<IGenericServerInfo> server);

    /// <summary>
    /// Used in polling, allows Game Resolvers to specify their own logic for retrieving a list of servers to poll. Allows filtering/limiting/etc.
    /// </summary>
    /// <returns></returns>
    public abstract Task<List<Server>> GetServers();

    /// <summary>
    /// Handles polling. By default, it calls GenericServerPoll with the result of GetServers, and then calls PollResult with the results. Can be overriden with custom logic.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> Poll()
    {
        var servers = await _steamServers.GenericServerPoll(await GetServers());
        await PollResult(servers);
        return servers.Count;
    }
    /// <summary>
    /// Handles Discovery. By default, it grabs the Steam Server List Query or makes a default one with AppId and filtering for Dedicated, non-empty servers, uses GenericServerDiscovery and calls DiscoveryResults. Can be overriden.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> Discovery()
    {
        var query = CustomQuery;
        if (query == null) query = SteamServerListQueryBuilder.New().AppID(AppId.ToString()).Dedicated().NotEmpty();

        var servers = await _steamServers.GenericServerDiscovery(query);
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
            var addresses = servers.Select(i => i.Address).ToList();
            var ports = servers.Select(i => i.Port).ToList();

            var entities = _genericServersContext.Servers
                .Where(a => addresses.Contains(a.Address) || ports.Contains(a.Port)).AsNoTracking()
                .Select(i => new { i.Address, i.Port, i.ServerID }).ToList(); // SQL IN

            var hashMapDictionary = new Dictionary<IPEndPoint, Guid>();
            foreach (var server in entities)
                hashMapDictionary.Add(new IPEndPoint(server.Address, server.Port), server.ServerID);

            foreach (var server in servers)
                if (hashMapDictionary.TryGetValue(new IPEndPoint(server.Address, server.Port), out var value))
                    server.ServerID = value;

#if DEBUG
            Console.WriteLine(
                $"Found {servers.Count(server => server.ServerID != Guid.Empty)} UUIDs Non-New Servers, out of {servers.Count}.");
#endif
        }

        foreach (var server in servers)
            if (server.ServerID == Guid.Empty)
                server.ServerID = Guid.NewGuid();
        await InsertGenericServer(servers.ToList<Server>());
        await _genericServersContext.BulkInsertOrUpdateAsync(servers);
        await transaction.CommitAsync();
    }

    private async Task InsertGenericServer(List<Server> servers)
    {
        var bulkConfig = new BulkConfig
        {
            PropertiesToExclude = new List<string> { "SearchVector" },
            PropertiesToExcludeOnUpdate = new List<string> { "FoundAt", "ServerID", "SearchVector" }
        };


        await _genericServersContext.BulkInsertOrUpdateAsync(servers, bulkConfig);
    }
}