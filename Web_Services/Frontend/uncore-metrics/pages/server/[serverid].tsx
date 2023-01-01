import { useRouter } from "next/router";
import * as React from "react";
import { styled } from "@mui/material/styles";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Unstable_Grid2";
import { useEffect } from "react";
import {
  ServerPlayerDataResponse,
  ServerRawPlayerDataResponse,
  ServerRawUptimeDataResponse,
  ServerUptimeDataResponse,
  SingleServerResponse,
} from "../../interfaces/server";
import { Server } from "../../interfaces/server";
import { PingLocation, LocationRequest } from "../../interfaces/location";
import styles from "./[serverid].module.css";
import {
  VictoryChart,
  VictoryLine,
  VictoryTheme,
  VictoryTooltip,
  VictoryVoronoiContainer,
} from "victory";
import FormControl from "@mui/material/FormControl";
import InputLabel from "@mui/material/InputLabel";
import Select, { SelectChangeEvent } from "@mui/material/Select";
import MenuItem from "@mui/material/MenuItem";

const Item = styled(Paper)(({ theme }) => ({
  backgroundColor: theme.palette.mode === "dark" ? "#1A2027" : "#fff",
  ...theme.typography.body2,
  padding: theme.spacing(1),
  textAlign: "center",
  color: theme.palette.text.secondary,
}));

const MAGIC_NUMBER_MAX_RESULTS = 250;

// TODO: Refactor using Next.js SSR
const Server = () => {
  const router = useRouter();
  const { serverid } = router.query;

  // Store in state so we can refresh every 10s
  const [ServerID, setServerID] = React.useState<string>(serverid as string);
  if (serverid && !ServerID) {
    setServerID(serverid as string);
  }

  // @ts-ignore
  const [data, setData] = React.useState<Server>(undefined); // @ts-ignore

  const [locations, setLocations] = React.useState<PingLocation[]>(undefined); // @ts-ignore

  const [uptimeData, setUptimeData] = React.useState<
    ServerUptimeDataResponse | ServerRawUptimeDataResponse
  >({
    data: [],
  }); // @ts-ignore
  const [playerData, setPlayerData] = React.useState<
    ServerPlayerDataResponse | ServerRawPlayerDataResponse
  >({
    data: [],
  }); // @ts-ignore
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string>("");
  const [playerDataTimeRange, setPlayerDataTimeRange] =
    React.useState<number>(72);
  const [uptimeDataTimeRange, setUptimeDataTimeRange] =
    React.useState<number>(72);

  useEffect(() => {
    const interval = setInterval(async () => {
      updateData(ServerID);
    }, 10000);
    const chartInterval = setInterval(async () => {
      if (ServerID) {
        if (playerDataTimeRange <= 1) {
          loadPlayerData(playerDataTimeRange, false);
        }
        if (uptimeDataTimeRange <= 1) {
          loadUptimeData(uptimeDataTimeRange, false);
        }
      }
    }, 30000);
    return () => {
      clearInterval(interval);
      clearInterval(chartInterval);
    };
  }, [ServerID]);
  //https://stackoverflow.com/questions/6108819/javascript-timestamp-to-relative-time
  function getRelativeTime(d1: Date, d2: Date = new Date()) {
    // in miliseconds
    
    var units: Record<string, number> = {
      year: 24 * 60 * 60 * 1000 * 365,
      month: (24 * 60 * 60 * 1000 * 365) / 12,
      day: 24 * 60 * 60 * 1000,
      hour: 60 * 60 * 1000,
      minute: 60 * 1000,
      second: 1000,
    };

    var rtf = new Intl.RelativeTimeFormat("en", { numeric: "auto" });
    var elapsed = d1.getTime() - d2.getTime();

    // "Math.abs" accounts for both "past" & "future" scenarios
    for (var u in units)
      if (Math.abs(elapsed) > units[u] || u == "second") // @ts-ignore
        return rtf.format(Math.round(elapsed / units[u]), u);
  }
  // https://stackoverflow.com/questions/11832914/how-to-round-to-at-most-2-decimal-places-if-necessary
  function SimpleRound(num: number, decimalPlaces = 0) {
    // @ts-ignore
    num = Math.round(num + "e" + decimalPlaces);
    return Number(num + "e" + -decimalPlaces);
  }
  const updateData = async (serverid: string) => {
    const serverFetchRequest = fetch(
      `https://api.uncore.app/v1/servers/${serverid}`
    );

    const serverDataResponse = await serverFetchRequest;
    const serverResponse: SingleServerResponse =
      await serverDataResponse.json();
    if (serverResponse.error) {
      setError(serverResponse.error.Message);
    }
    if (serverResponse.data) {
      setData(serverResponse?.data);
    }
    var serverLocationFetch = await fetch(
      `https://api.uncore.app/v1/locations`
    );
    const serverLocationResponse: LocationRequest =
      await serverLocationFetch.json();
    if (serverLocationResponse.data) {
      setLocations(serverLocationResponse.data);
    }
    setLoading(false);
  };
  const handlePlayerDataTimeRangeChange = async (event: SelectChangeEvent) => {
    let hours = Number.parseInt(event.target.value);
    setPlayerDataTimeRange(hours);
    loadPlayerData(hours);
  };
  const loadPlayerData = async (hours: number, clear: boolean = true) => {
    if (clear) {
      setPlayerData({ data: [] });
    }
    let days = hours / 24;
    let maxHoursGroupBy: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
    let serverPlayerDataRequest;
    if (hours < 12 && hours != -1) {
      serverPlayerDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/playerdataraw/${serverid}?hours=${hours}`
      );
      const playerDataResponse: ServerRawPlayerDataResponse =
        await serverPlayerDataRequest.json();
      if (playerDataResponse.data) {
        setPlayerData(playerDataResponse);
      }
      return;
    }
    if (hours == -1) {
      days = MAGIC_NUMBER_MAX_RESULTS - 1;
      hours = days * 24;
      maxHoursGroupBy = 24;
    }
    if (maxHoursGroupBy >= 12) {
      let groupby: number = Math.ceil(days / MAGIC_NUMBER_MAX_RESULTS);
      serverPlayerDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/playerdata1d/${serverid}?days=${days}${
          groupby > 1 ? "&groupby=" + groupby : ""
        }`
      );
    } else {
      let groupby: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
      if (hours <= 24) {
        groupby = 0;
      }
      serverPlayerDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/playerdata/${serverid}?hours=${hours}${
          groupby >= 1 ? "&groupby=" + groupby : ""
        }`
      );
    }
    const playerDataResponse: ServerPlayerDataResponse =
      await serverPlayerDataRequest.json();
    if (playerDataResponse.data) {
      setPlayerData(playerDataResponse);
    }
  };
  const handleUptimeDataTimeRangeChange = async (event: SelectChangeEvent) => {
    let hours = Number.parseInt(event.target.value);
    setUptimeDataTimeRange(hours);
    loadUptimeData(hours);
  };
  const loadUptimeData = async (hours: number, clear: boolean = true) => {
    if (clear) {
      setUptimeData({ data: [] });
    }
    let days = hours / 24;
    let serverUptimeDataRequest;
    let maxHoursGroupBy: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
    if (hours < 12 && hours != -1) {
      serverUptimeDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/uptimedataraw/${serverid}?hours=${hours}`
      );
      const uptimeDataResponse: ServerRawUptimeDataResponse =
        await serverUptimeDataRequest.json();
      if (uptimeDataResponse.data) {
        setUptimeData(uptimeDataResponse);
      }
      return;
    }
    if (hours == -1) {
      days = MAGIC_NUMBER_MAX_RESULTS - 1;
      hours = days * 24;
      maxHoursGroupBy = 24;
    }
    if (maxHoursGroupBy >= 12) {
      let groupby: number = Math.ceil(days / MAGIC_NUMBER_MAX_RESULTS);
      serverUptimeDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/uptimedata1d/${serverid}?days=${days}${
          groupby > 1 ? "&groupby=" + groupby : ""
        }`
      );
    } else {
      let groupby: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
      if (hours <= 24) {
        groupby = 0;
      }
      serverUptimeDataRequest = await fetch(
        `https://api.uncore.app/v1/servers/uptimedata/${serverid}?hours=${hours}${
          groupby >= 1 ? "&groupby=" + groupby : ""
        }`
      );
    }
    const uptimeDataResponse: ServerUptimeDataResponse =
      await serverUptimeDataRequest.json();
    if (uptimeDataResponse.data) {
      setUptimeData(uptimeDataResponse);
    }
  };
  const initialLoad = async () => {
    await updateData(serverid as string);
    var initLoadPlayerDataPromise = loadPlayerData(playerDataTimeRange);
    var initLoadUptimeDataPromise = loadUptimeData(uptimeDataTimeRange);
    await Promise.all([initLoadPlayerDataPromise, initLoadUptimeDataPromise]);
  };

  if (loading && !error && !data && serverid) {
    initialLoad();
  }
  if (loading) {
    return <div>Loading...</div>;
  }
  if (error) {
    return <div>Error: {error}</div>;
  }
  return (
    <div>
      <Grid container spacing={3} padding={3}>
        <Grid xs>
          <Item>
            <p>{data.game}</p>
          </Item>
        </Grid>
        <Grid xs={6}>
          <Item>
            <p>{data.name}</p>
          </Item>
        </Grid>
        <Grid xs>
          <Item>
            <p>{data.map}</p>
          </Item>
        </Grid>
      </Grid>
      <Grid container spacing={3} padding={3}></Grid>
      <Grid container spacing={3} sx={{ flexGrow: 1 }}>
        <Grid xs md={6} mdOffset={0} padding={3}>
          <Item className={styles.serverPropertyList}>
            <dl>
              <dt>Server GUID: </dt>
              <dd>{data.serverID}</dd>
              <dt>Server Address: </dt>
              <dd>
                {data.ipAddress}:{data.port}
              </dd>
              <dt>Query Address: </dt>
              <dd>
                {data.ipAddress}:{data.queryPort}
              </dd>
              <dt>Players: </dt>
              <dd>
                {data.players}/{data.maxPlayers}
              </dd>
              <dt>Status: </dt>
              <dd>{data.isOnline ? "Online" : "Offline"}</dd>
              <dt>Country: </dt>
              <dd>{data.country}</dd>
              <dt>Continent: </dt>
              <dd>{data.continent}</dd>
              <dt>Timezone: </dt>
              <dd>{data.timezone}</dd>
              <dt>Last Check: </dt>
              <dd>{getRelativeTime(new Date(data.lastCheck))}</dd>
              <dt>Found At: </dt>
              <dd>
                {new Date(data.foundAt).toLocaleDateString()} -{" "}
                {getRelativeTime(new Date(data.foundAt))}
              </dd>
              <dt>Next Check </dt>
              <dd>{getRelativeTime(new Date(data.nextCheck))}</dd>
              <dt>ISP: </dt>
              <dd>{data.isp}</dd>
              <dt>ASN: </dt>
              <dd>{data.asn}</dd>
              <dt>Server Is Dead (not responding for 24 hours):</dt>
              <dd>{data.isOnline ? "Alive" : "Dead"}</dd>
              <dt>
                Unsuccessful Retries (failed to respond x checks in a row):
              </dt>
              <dd>{data.retriesUsed}</dd>
            </dl>
          </Item>
        </Grid>
        <Grid xs={4} mdOffset="auto">
          <Item>
            <h2 style={{ float: "left", fontSize: "200%" }}>
              Players over time
            </h2>
            <FormControl size="medium" style={{ width: "50%", float: "right" }}>
              <InputLabel id="player-data-period-select-label">
                Time period
              </InputLabel>
              <Select
                labelId="player-data-period-select-label"
                id="player-data-period-select"
                label="Time Period"
                onChange={handlePlayerDataTimeRangeChange}
                value={playerDataTimeRange.toString()}
              >
                <MenuItem value={1}>1 hour (Live)</MenuItem>
                <MenuItem value={6}>6 hours</MenuItem>
                <MenuItem value={12}>12 hours</MenuItem>
                <MenuItem value={24}>1 Day</MenuItem>
                <MenuItem value={72}>3 Days</MenuItem>
                <MenuItem value={336}>14 Days</MenuItem>
                <MenuItem value={720}>1 Month</MenuItem>
                <MenuItem value={2160}>3 Months</MenuItem>
                <MenuItem value={4320}>6 Months</MenuItem>
                <MenuItem value={8760}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart
              domainPadding={{ y: 20, x: 5 }}
              containerComponent={
                <VictoryVoronoiContainer
                  labels={({ datum }) =>
                    `${new Date(datum.x).toLocaleString()}\n${datum.y} players`
                  }
                  labelComponent={
                    <VictoryTooltip
                      cornerRadius={0}
                      flyoutStyle={{
                        fill: "black",
                        stroke: "black",
                        opacity: "0.10",
                      }}
                    />
                  }
                  theme={VictoryTheme.material}
                />
              }
            >
              <VictoryLine
                interpolation="natural"
                style={{ labels: {}, data: { stroke: "skyblue" } }}
                theme={VictoryTheme.material}
                scale={{ x: "time" }}
                data={playerData?.data?.map((d: any) => {
                  return {
                    x: new Date(d.checkTime ?? d.averageTime),
                    y: SimpleRound(d.players ?? d.playerAvg),
                  };
                })}
              />
            </VictoryChart>
          </Item>
        </Grid>
        <Grid xs md={6} mdOffset={0} padding={3}>
          <Item className={styles.serverPropertyList}>
            <dl>
              {data.serverPings.map((ping) => {
                return (
                  <React.Fragment key={ping.locationID}>
                    <dt>
                      {locations.find(
                        (location) => location.locationID == ping.locationID
                      )?.locationName ?? ping.locationID}
                    </dt>
                    <dd>
                      {ping.failed && ping.pingMs == 0 ? "Server blocks Pings": `${ping.pingMs}ms`} -{" "}
                      {getRelativeTime(new Date(ping.lastCheck))}
                    </dd>
                  </React.Fragment>
                );
              })}
            </dl>
          </Item>
        </Grid>
        <Grid xs={4} mdOffset="auto">
          <Item>
            <h2 style={{ float: "left", fontSize: "200%" }}>Uptime Data</h2>
            <FormControl size="medium" style={{ width: "50%", float: "right" }}>
              <InputLabel id="player-data-period-select-label">
                Time period
              </InputLabel>
              <Select
                labelId="player-data-period-select-label"
                id="player-data-period-select"
                label="Time Period"
                onChange={handleUptimeDataTimeRangeChange}
                value={uptimeDataTimeRange.toString()}
              >
                <MenuItem value={1}>1 hour (Live)</MenuItem>
                <MenuItem value={6}>6 hours</MenuItem>
                <MenuItem value={12}>12 hours</MenuItem>
                <MenuItem value={24}>1 Day</MenuItem>
                <MenuItem value={72}>3 Days</MenuItem>
                <MenuItem value={336}>14 Days</MenuItem>
                <MenuItem value={720}>1 Month</MenuItem>
                <MenuItem value={2160}>3 Months</MenuItem>
                <MenuItem value={4320}>6 Months</MenuItem>
                <MenuItem value={8760}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart
              domainPadding={{ y: 20, x: 5 }}
              containerComponent={
                <VictoryVoronoiContainer
                  labels={({ datum }) =>
                    `${new Date(datum.x).toLocaleString()}\n${
                      datum.y
                    }% successful checks\n ${datum.online}/${datum.ping}`
                  }
                  labelComponent={
                    <VictoryTooltip
                      cornerRadius={0}
                      flyoutStyle={{
                        fill: "black",
                        stroke: "black",
                        opacity: "0.10",
                      }}
                    />
                  }
                  theme={VictoryTheme.material}
                />
              }
            >
              <VictoryLine
                interpolation="natural"
                theme={VictoryTheme.material}
                scale={{ x: "time" }}
                style={{ labels: {}, data: { stroke: "skyblue" } }}
                data={uptimeData?.data?.map((d: any) => {
                  return {
                    x: new Date(d.averageTime ?? d.checkTime),
                    y: d.uptime
                      ? SimpleRound(d.uptime, 2)
                      : d.isOnline
                      ? 100
                      : 0,
                    ping: d.pingCount ?? (d.isOnline ? 1 : 0),
                    online: d.onlineCount ?? (d.isOnline ? 1 : 0),
                  };
                })}
                domain={{ y: [0, 100] }}
              />
            </VictoryChart>
          </Item>
        </Grid>
      </Grid>
    </div>
  );
};

export default Server;
