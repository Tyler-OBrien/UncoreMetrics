using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse;

public interface IClickHouseService
{
    public Task Insert(IEnumerable<ClickHouseGenericServer> servers, CancellationToken token = default);


    public Task<double> GetServerUptime(string serverId, int lastHours = 0, CancellationToken token = default);

    public Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours,
        CancellationToken token = default);

    public  Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, int hoursGroupBy,
        CancellationToken token = default);

    public Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours,
        CancellationToken token = default);

    public Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours, int hoursGroupBy,
        CancellationToken token = default);


    public Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverID, int lastDays,
        CancellationToken token = default);

    public Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverID, int lastDays, int daysGroupBy,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerData(string serverId, int lastHours,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerData(string serverId, int lastHours, int hoursGroupBy, CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours, int hoursGroupBy, CancellationToken token = default);



    public Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays,
        CancellationToken token = default);

    public Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays, int daysGroupBy, CancellationToken token = default);
}