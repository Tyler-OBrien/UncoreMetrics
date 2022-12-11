Generate new migrations:

>  dotnet ef migrations add AddScrapeJobs  --context ServersContext --output-dir "Migrations\ServerContext" --startup-project ..\\..\\Web_Services\API

Apply Migrations to get started:

> dotnet ef migrations script --startup-project  ..\\..\\Web_Services\API or  dotnet ef database update --startup-project   ..\\..\\Web_Services\API

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


Materialized Views:

CREATE MATERIALIZED VIEW generic_server_stats_players_mv
ENGINE = AggregatingMergeTree
PARTITION BY toYYYYMM(average_time)
ORDER BY (average_time, server_id)
AS SELECT
   server_id,
   appid,
   avgState(players) as players_avg,
   minState(players) as players_min,
   maxState(players) as players_max,
   average_time
FROM generic_server_stats
GROUP BY
   server_id,
   toStartOfInterval(current_check_time, INTERVAL 30 minute) as average_time,
   current_check_time,
   appid


CREATE MATERIALIZED VIEW generic_server_stats_uptime_mv
ENGINE = AggregatingMergeTree
PARTITION BY toYYYYMM(average_time)
ORDER BY (average_time, server_id)
AS SELECT
   server_id,
   appid,
   countState(CASE WHEN is_online THEN 1 END) as online_count,
   countState() as ping_count,
   average_time
FROM generic_server_stats
GROUP BY
   server_id,
   toStartOfInterval(current_check_time, INTERVAL 30 minute) as average_time,
   current_check_time,
   appid
