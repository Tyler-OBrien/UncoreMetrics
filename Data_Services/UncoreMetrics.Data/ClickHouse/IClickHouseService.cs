using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse;

public interface IClickHouseService
{
    public Task Insert(IEnumerable<ClickHouseGenericServer> servers, CancellationToken token = default);


    public Task<double> GetServerUptime(string serverId, int lastHours = 0, CancellationToken token = default);


    public  Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, int hoursGroupBy,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerCountPer30Minutes(string serverId, int lastHours,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerCount(string serverId, int lastHours, int hoursGroupBy, CancellationToken token = default);
}