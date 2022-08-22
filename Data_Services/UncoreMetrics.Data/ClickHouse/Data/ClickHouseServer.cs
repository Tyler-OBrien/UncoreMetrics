using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse.Data;

public class ClickHouseServer
{
    public const string TableName = "generic_server_stats";

    public ClickHouseServer(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; set; }

    public ClickHouseConnection CreateConnection()
    {
        return new(ConnectionString);
    }


    public async Task Insert(IEnumerable<ClickHouseGenericServer> servers, CancellationToken token = default)
    {
        using var connection = CreateConnection();
        using var bulkCopyInterface = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = TableName,
            BatchSize = 100000
        };

        // Example data to insert
        await bulkCopyInterface.WriteToServerAsync(ClickHouseGenericServer.ToDatabase(servers), token);
    }

    public async Task<double> GetServerUptime(string serverId, int lastHours = 0, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverId);
        command.CommandText =
           "SELECT countMerge(online_count) / countMerge(ping_count) * 100 FROM generic_server_stats_uptime_mv Where server_id = {serverId:UUID}";
        if (lastHours != 0)
        {
            command.AddParameter("lastHours", "Int32", lastHours);
            command.CommandText += " and average_time  > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR)";
        }

        command.CommandText += ";";
        var result = (await command.ExecuteScalarAsync(token))?.ToString();
        if (result != null)
            if (double.TryParse(result, out var uptime))
                return uptime;

        return -1;
    }
    public async Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, int hoursGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);
        command.AddParameter("hoursGroupBy", "Int32", hoursGroupBy);

        command.CommandText =
            "Select server_id, appid, countMerge(online_count) / countMerge(ping_count) * 100 as uptime, average_time from generic_server_stats_uptime_mv where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY server_id, appid, toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result));
        }

        return data;
    }

    public async Task<List<ClickHousePlayerData>> GetPlayerDataPer30Minutes(string serverID, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);

        command.CommandText =
            "Select server_id, appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY server_id, appid, average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(PlayerDataFromReader(result));
        }

        return data;
    }
    public async Task<List<ClickHousePlayerData>> GetPlayerData(string serverID, int lastHours, int hoursGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);
        command.AddParameter("hoursGroupBy", "Int32", hoursGroupBy);

        command.CommandText =
            "Select server_id, appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY server_id, appid, toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(PlayerDataFromReader(result));
        }

        return data;
    }

    private ClickHousePlayerData PlayerDataFromReader(DbDataReader reader)
    {
        return new ClickHousePlayerData()
        {
            ServerId = reader.GetGuid("server_id"),
            AppId = (ulong)reader.GetValue("appid"),
            PlayerAvg = reader.GetDouble("players_avg"),
            PlayersMin = (uint)reader.GetValue("players_min"),
            PlayersMax = (uint)reader.GetValue("players_max"),
            AverageTime = reader.GetDateTime("average_time")
        };
    }
    private ClickHouseUptimeData UptimeDataFromReader(DbDataReader reader)
    {
        return new ClickHouseUptimeData()
        {
            ServerId = reader.GetGuid("server_id"),
            AppId = (ulong)reader.GetValue("appid"),
            Uptime = reader.GetDouble("uptime"),
            AverageTime = reader.GetDateTime("average_time")
        };
    }
}