using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Data.ClickHouse.Data
{
    public class ClickHouseServer
    {
        public ClickHouseServer(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public const string TableName = "generic_server_stats";

        public ClickHouseConnection CreateConnection() => new ClickHouseConnection(ConnectionString);



        public async Task Insert(IEnumerable<ClickHouseGenericServer> servers)
        {
            using var connection = CreateConnection();
            using var bulkCopyInterface = new ClickHouseBulkCopy(connection)
            {
                DestinationTableName = TableName,
                BatchSize = 100000,
            };

            // Example data to insert
            await bulkCopyInterface.WriteToServerAsync(ClickHouseGenericServer.ToDatabase(servers));
        }

        public async Task<float> GetServerUptime(string serverId, int lastHours = 0)
        {
            using var connection = CreateConnection();

            using var command = connection.CreateCommand();
            command.AddParameter("serverId", "UUID", serverId);
            command.CommandText =
                "SELECT COUNT(CASE WHEN is_online THEN 1 END) / COUNT(*) * 100 FROM generic_server_stats Where server_id = {serverId:UUID}";
            if (lastHours != 0)
            {
                command.AddParameter("lastHours", "Int32", lastHours);
                command.CommandText += " and current_check_time > DATE_SUB(NOW(), INTERVAL {lastHours:Int32} HOUR)";
            }
            command.CommandText += ";";
            var result = (await command.ExecuteScalarAsync())?.ToString();
            if (result != null)
            {
                if (float.TryParse(result, out var uptime))
                    return uptime;
            }

            return -1;
        }
    }
}
