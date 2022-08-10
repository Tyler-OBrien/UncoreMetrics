using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse;

public interface IClickHouseService
{
    public Task Insert(IEnumerable<ClickHouseGenericServer> servers);


    public Task<float> GetServerUptime(string serverId, int lastHours = 0);
}