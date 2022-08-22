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


    public Task Insert(IEnumerable<ClickHouseGenericServer> servers)
    {
        return _server.Insert(servers);
    }


    public Task<float> GetServerUptime(string serverId, int lastHours = 0)
    {
        return _server.GetServerUptime(serverId, lastHours);
    }

    public Task<List<ClickHousePlayerData>> GetPlayerCountPer30Minutes(string serverId, int lastHours,
        CancellationToken token = default) => _server.GetPlayerCountPer30Minutes(serverId, lastHours, token);

    public  Task<List<ClickHousePlayerData>> GetPlayerCount(string serverId, int lastHours, int hoursGroupBy, CancellationToken token = default) => _server.GetPlayerCount(serverId, lastHours, hoursGroupBy, token);

}