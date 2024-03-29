﻿using System.Data;
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
            if (double.TryParse(result, out var uptime) && double.IsNormal(uptime))
                return uptime;

        return -1;
    }

    public async Task<List<ClickHouseRawUptimeData>> GetUptimeDataRaw(string serverID, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);

        command.CommandText =
            "Select server_id, is_online, current_check_time from generic_server_stats where server_id = {serverId:UUID} and current_check_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) Order by current_check_time desc LIMIT 1000";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseRawUptimeData> data = new List<ClickHouseRawUptimeData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(RawUptimeDataFromReader(result));
        }

        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);

        command.CommandText =
            "Select server_id, appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY server_id, appid, average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result));
        }

        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeData(string serverID, int lastHours, int hoursGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);
        command.AddParameter("hoursGroupBy", "Int32", hoursGroupBy);

        command.CommandText =
            "Select server_id, appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY server_id, appid, toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastHours / hoursGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result));
        }
        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverId, int lastDays, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverId);
        command.AddParameter("lastDays", "Int32", lastDays);

        command.CommandText =
            "Select server_id, appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv_day where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastDays:Int32} DAY) GROUP BY server_id, appid, average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastDays);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result));
        }

        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeData1d(string serverId, int lastDays, int daysGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverId);
        command.AddParameter("lastDays", "Int32", lastDays);
        command.AddParameter("daysGroupBy", "Int32", daysGroupBy);

        command.CommandText =
            "Select server_id, appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv_day where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastDays:Int32} DAY) GROUP BY server_id, appid, toStartOfInterval(average_time, INTERVAL {daysGroupBy:Int32} DAY) as average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastDays / daysGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result));
        }
        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("lastHours", "Int32", lastHours);
        bool hasAppid = false;
        if (appId == null || appId == 0)
        {
            command.CommandText =
                "Select countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY  average_time Order by average_time desc LIMIT 500;";
        }
        else
        {
            command.AddParameter("appId", "UInt64", appId);
            hasAppid = true;
            command.CommandText =
                "Select appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where appid = {appId:UInt64} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY appid, average_time Order by average_time desc LIMIT 500;";
        }

        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result, false, hasAppid));
        }

        return data;
    }

    public async Task<List<ClickHouseUptimeData>> GetUptimeDataOverall(ulong? appId, int lastHours, int hoursGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("lastHours", "Int32", lastHours);
        command.AddParameter("hoursGroupBy", "Int32", hoursGroupBy);
        bool hasAppid = false;
        if (appId == null || appId == 0)
        {
            command.CommandText =
                "Select countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY   toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time Order by average_time desc LIMIT 500;";
        }
        else
        {
            hasAppid = true;
            command.AddParameter("appId", "UInt64", appId);
            command.CommandText =
                "Select appid, countMerge(online_count) as online_count, countMerge(ping_count) as ping_count, average_time from generic_server_stats_uptime_mv where appid = {appId:UInt64} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY appid, toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time Order by average_time desc LIMIT 500;";
        }



        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseUptimeData> data = new List<ClickHouseUptimeData>(lastHours / hoursGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(UptimeDataFromReader(result, false, hasAppid));
        }
        return data;
    }

    public async Task<List<ClickHouseRawPlayerData>> GetPlayerDataRaw(string serverID, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverID);
        command.AddParameter("lastHours", "Int32", lastHours);

        command.CommandText =
            "Select server_id, players, current_check_time from generic_server_stats where server_id = {serverId:UUID} and current_check_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) Order by current_check_time desc LIMIT 1000";

        var result = await command.ExecuteReaderAsync(token);
        List<ClickHouseRawPlayerData> data = new List<ClickHouseRawPlayerData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(RawPlayerDataFromReader(result));
        }

        return data;
    }


    public async Task<List<ClickHousePlayerData>> GetPlayerData(string serverID, int lastHours, CancellationToken token = default)
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
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastHours / hoursGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(PlayerDataFromReader(result));
        }

        return data;
    }

    public async Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverId);
        command.AddParameter("lastDays", "Int32", lastDays);

        command.CommandText =
            "Select server_id, appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv_day where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastDays:Int32} DAY) GROUP BY server_id, appid, average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastDays);
        while (await result.ReadAsync(token))
        {
            data.Add(PlayerDataFromReader(result));
        }

        return data;
    }
    public async Task<List<ClickHousePlayerData>> GetPlayerData1d(string serverId, int lastDays, int daysGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("serverId", "UUID", serverId);
        command.AddParameter("lastDays", "Int32", lastDays);
        command.AddParameter("daysGroupBy", "Int32", daysGroupBy);

        command.CommandText =
            "Select server_id, appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv_day where server_id = {serverId:UUID} and average_time > DATE_SUB(NOW(), INTERVAL {lastDays:Int32} DAY) GROUP BY server_id, appid, toStartOfInterval(average_time, INTERVAL {daysGroupBy:Int32} DAY) as average_time Order by average_time desc LIMIT 500;";
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastDays / daysGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(PlayerDataFromReader(result));
        }

        return data;
    }


    public async Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("lastHours", "Int32", lastHours);
        bool hasAppid = false;
        if (appId == null || appId == 0)
            // TODO: THESE QUERIES SHOULD BE REDONE AT SOME POINT
        {
           // command.CommandText =
           //     "Select avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY average_time Order by average_time desc LIMIT 500;";

            command.CommandText =
                "Select sum(temp.players_max) as players_max, temp.average_time as average_time from (select (maxMerge(players_max)) as players_max, average_time from generic_server_stats_players_mv_overall where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) group by average_time, server_id order by average_time desc) temp group by temp.average_time order by average_time desc";

        }
        else
        {
            hasAppid = true;
            command.AddParameter("appId", "UInt64", appId);
           // command.CommandText =
           //     "Select appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where appid = {appId:UInt64} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY appid, average_time Order by average_time desc LIMIT 500;";
           command.CommandText =
               "Select sum(temp.players_max) as players_max, temp.appid as appid, temp.average_time as average_time from (select appid, (maxMerge(players_max)) as players_max, average_time from generic_server_stats_players_mv_overall where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) and appid = {appId:UInt64} group by appid, average_time, server_id order by average_time desc) temp group by temp.appid, temp.average_time order by average_time desc";
        }
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastHours * 2);
        while (await result.ReadAsync(token))
        {
            data.Add(OverallPlayerDataFromReader(result, hasAppid));
        }

        return data;
    }
    public async Task<List<ClickHousePlayerData>> GetPlayerDataOverall(ulong? appId, int lastHours, int hoursGroupBy, CancellationToken token = default)
    {
        using var connection = CreateConnection();

        using var command = connection.CreateCommand();
        command.AddParameter("lastHours", "Int32", lastHours);
        command.AddParameter("hoursGroupBy", "Int32", hoursGroupBy);
        bool hasAppid = false;
        if (appId == null || appId == 0)
        // TODO: THESE QUERIES SHOULD BE REDONE AT SOME POINT
        {
            // command.CommandText =
            //     "Select avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY average_time Order by average_time desc LIMIT 500;";

            command.CommandText =
                "Select sum(temp.players_max) as players_max, temp.average_time as average_time from (select (maxMerge(players_max)) as players_max, average_time from generic_server_stats_players_mv_overall where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) group by  toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time, server_id order by average_time desc) temp group by temp.average_time order by average_time desc";
        }
        else
        {
            hasAppid = true;
            command.AddParameter("appId", "UInt64", appId);
            // command.CommandText =
            //     "Select appid, avgMerge(players_avg) as players_avg, minMerge(players_min) as players_min, maxMerge(players_max) as players_max, average_time from generic_server_stats_players_mv where appid = {appId:UInt64} and average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) GROUP BY appid, average_time Order by average_time desc LIMIT 500;";
            command.CommandText =
                "Select sum(temp.players_max) as players_max, temp.appid as appid, temp.average_time as average_time from (select appid, (maxMerge(players_max)) as players_max, average_time from generic_server_stats_players_mv_overall where average_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR) and appid = {appId:UInt64} group by appid,  toStartOfInterval(average_time, INTERVAL {hoursGroupBy:Int32} HOUR) as average_time, server_id order by average_time desc) temp group by temp.appid, temp.average_time order by average_time desc";
        }
        var result = await command.ExecuteReaderAsync(token);
        List<ClickHousePlayerData> data = new List<ClickHousePlayerData>(lastHours / hoursGroupBy);
        while (await result.ReadAsync(token))
        {
            data.Add(OverallPlayerDataFromReader(result, hasAppid));
        }

        return data;
    }


    private ClickHousePlayerData OverallPlayerDataFromReader(DbDataReader reader,  bool hasAppId)
    {
        return new ClickHousePlayerData()
        {
            AppId = hasAppId ? (ulong)reader.GetValue("appid") : 0,
            PlayersMax = (uint)(ulong)reader.GetValue("players_max"),
            AverageTime = reader.GetDateTime("average_time"),
        };
    }

    private ClickHousePlayerData PlayerDataFromReader(DbDataReader reader)
    {
        return new ClickHousePlayerData()
        {
            ServerId =  reader.GetGuid("server_id"),
            AppId =  (ulong)reader.GetValue("appid"),
            PlayerAvg =  reader.GetDouble("players_avg"),
            PlayersMin =  (uint)reader.GetValue("players_min"),
            PlayersMax =  (uint)reader.GetValue("players_max"),
            AverageTime = reader.GetDateTime("average_time"),
        };
    }
    private ClickHouseRawPlayerData RawPlayerDataFromReader(DbDataReader reader)
    {
        return new ClickHouseRawPlayerData()
        {
            ServerId = reader.GetGuid("server_id"),
            Players = (uint)reader.GetValue("players"),
            CheckTime = reader.GetDateTime("current_check_time"),
        };
    }
    private ClickHouseUptimeData UptimeDataFromReader(DbDataReader reader, bool hasServerID = true, bool hasAppID = true)
    {
        return new ClickHouseUptimeData()
        {
            ServerId = hasServerID ? reader.GetGuid("server_id") : Guid.Empty,
            AppId = hasAppID ? (ulong)reader.GetValue("appid") : 0,
            OnlineCount = (ulong)reader.GetValue("online_count"),
            PingCount = (ulong)reader.GetValue("ping_count"),
            AverageTime = reader.GetDateTime("average_time")
        };
    }
    private ClickHouseRawUptimeData RawUptimeDataFromReader(DbDataReader reader)
    {
        return new ClickHouseRawUptimeData()
        {
            ServerId = reader.GetGuid("server_id"),
            IsOnline = reader.GetBoolean("is_online"),
            CheckTime = reader.GetDateTime("current_check_time"),
        };
    }
}