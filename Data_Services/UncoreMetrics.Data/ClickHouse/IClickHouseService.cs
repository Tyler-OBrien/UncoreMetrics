using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse;

public interface IClickHouseService
{
    public Task Insert(IEnumerable<ClickHouseGenericServer> servers);


    public Task<float> GetServerUptime(string serverId, int lastHours = 0);

    public Task<List<ClickHousePlayerData>> GetPlayerCountPer30Minutes(string serverId, int lastHours,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerCount(string serverId, int lastHours, int hoursGroupBy, CancellationToken token = default);
}