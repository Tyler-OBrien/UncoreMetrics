using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
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
            Console.WriteLine(bulkCopyInterface.RowsWritten);
        }
    }
}
