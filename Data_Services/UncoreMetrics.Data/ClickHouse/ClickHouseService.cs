using Microsoft.Extensions.Options;
using UncoreMetrics.Data.ClickHouse.Data;
using UncoreMetrics.Data.ClickHouse.Models;
using UncoreMetrics.Data.Configuration;

namespace UncoreMetrics.Data.ClickHouse;

public class ClickHouseService : IClickHouseService
{
    private readonly BaseConfiguration _baseConfiguration;

    private readonly ClickHouseServer _server;


    public ClickHouseService(IOptions<BaseConfiguration> baseConfigurationOptions)
    {
        _baseConfiguration = baseConfigurationOptions.Value;
        _server = new ClickHouseServer(_baseConfiguration.ClickhouseConnectionString);
    }


    public Task Insert(IEnumerable<ClickHouseGenericServer> servers, CancellationToken token = default)
    {
        return _server.Insert(servers, token);
    }


    public Task<double> GetServerUptime(string serverId, int lastHours = 0, CancellationToken token = default)
    {
        return _server.GetServerUptime(serverId, lastHours, token);
    }


    public Task<List<ClickHouseRawUptimeData>> GetUptimeDataRaw(string serverId, int lastHours, CancellationToken token = default) => _server.GetUptimeDataRaw(serverId, lastHours, token);


    public Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, CancellationToken token = default) => _server.GetUptimeData(serverID, lastHours, token);

    public Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, int hoursGroupBy,
        CancellationToken token = default) => _server.GetUptimeData(serverID, lastHours, hoursGroupBy, token);

    public  Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours, CancellationToken token = default) =>_server.GetUptimeDataOverall(appId, lastHours, token);
    

    public Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours, int hoursGroupBy, CancellationToken token = default) => _server.GetUptimeDataOverall(appId, lastHours, hoursGroupBy, token);


    public Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverId, int lastDays,
        CancellationToken token = default) => _server.GetUptimeData1d(serverId, lastDays, token);

    public Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverId, int lastDays, int daysGroupBy,
        CancellationToken token = default) => _server.GetUptimeData1d(serverId, lastDays, daysGroupBy, token);


    public Task<List<ClickHouseRawPlayerData>> GetPlayerDataRaw(string serverId, int lastHours,
        CancellationToken token = default) => _server.GetPlayerDataRaw(serverId, lastHours, token);

    public Task<List<ClickHousePlayerData>> GetPlayerData(string serverId, int lastHours,
        CancellationToken token = default) => _server.GetPlayerData(serverId, lastHours, token);

    public  Task<List<ClickHousePlayerData>> GetPlayerData(string serverId, int lastHours, int hoursGroupBy, CancellationToken token = default) => _server.GetPlayerData(serverId, lastHours, hoursGroupBy, token);

    public Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours,
        CancellationToken token = default) => _server.GetPlayerDataOverall(appId, lastHours, token);

    public Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours, int hoursGroupBy, CancellationToken token = default) => _server.GetPlayerDataOverall(appId, lastHours, hoursGroupBy, token);

    public Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays,
        CancellationToken token = default) => _server.GetPlayerData1d(serverId, lastDays, token);

    public Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays, int daysGroupBy,
        CancellationToken token = default) => _server.GetPlayerData1d(serverId, lastDays, daysGroupBy, token);
}