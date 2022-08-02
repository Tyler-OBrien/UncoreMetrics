Generate new migrations:

>  dotnet ef migrations add InitialCreate  --context ServersContext --output-dir "Migrations\ServerContext" --startup-project ..\..\Collector_Services\Steam_Collector

Apply Migrations to get started:

> dotnet ef migrations script --startup-project  ..\..\Collector_Services\Steam_Collector or  dotnet ef database update --startup-project   ..\..\Collector_Services\Steam_Collector

Clickhouse:

CREATE TABLE generic_server_stats
(
 `server_id` UUID,
 `appid` UInt64,
 `address_ipv4` IPv4, 
 `address_ipv6` IPv6,
 `port` UInt16,
 `query_port` UInt16, 
 `players` UInt32,
 `max_players` UInt32,
 `retries_used` UInt8,
 `visibility` Bool,
 `environment` char, 
 `vac` Bool,
 `keywords` String,
 `Server_steamid` UInt64, 
 `asn` UInt32, 
 `country` String,
 `continent` Enum('Unknown', 'AF', 'AN', 'AS', 'EU', 'NA', 'OC', 'SA'), 
 `is_online` Bool, 
 `server_dead` Bool, 
 `failed_checks` UInt32, 
 `last_check` DateTime, 
 `next_check` DateTime, 
 `current_check_time` DateTime
)
ENGINE = MergeTree
PRIMARY KEY (current_check_time, server_id)
PARTITION BY toYYYYMM(current_check_time)
ORDER BY (current_check_time, server_id);