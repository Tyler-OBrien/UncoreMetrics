import { useRouter } from "next/router";
import * as React from "react";
import { styled } from "@mui/material/styles";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Unstable_Grid2";
import { useEffect } from "react";
import {
  ClickhousePlayerData,
  ClickHouseUptimeData,
  ServerPlayerDataResponse,
  ServerUptimeDataResponse,
  SingleServerResponse,
} from "../../interfaces/server";
import { Server } from "../../interfaces/server";
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
  const [uptimeData, setUptimeData] = React.useState<ServerUptimeDataResponse>({
    data: [],
  }); // @ts-ignore
  const [playerData, setPlayerData] = React.useState<ServerPlayerDataResponse>({
    data: [],
  }); // @ts-ignore
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string>("");
  const [playerDataTimeRange, setPlayerDataTimeRange] =
    React.useState<number>(3);
  const [uptimeDataTimeRange, setUptimeDataTimeRange] =
    React.useState<number>(3);

  useEffect(() => {
    const interval = setInterval(() => updateData(ServerID), 10000);
    return () => {
      clearInterval(interval);
    };
  }, [ServerID]);

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
    setLoading(false);
  };
  const handlePlayerDataTimeRangeChange = async (event: SelectChangeEvent) => {
    let days = Number.parseInt(event.target.value);
    setPlayerDataTimeRange(days);
    loadPlayerData(days);
  };
  const loadPlayerData = async (days: number) => {
    setPlayerData({ data: [] });
    let hours = days * 24;
    let groupby: number = 0.5;
    let playerDataEndpoint = "playerdata";
    if (days == -1) {
      days = 500;
      hours = days * 24;
    }
    if (hours > 96) {
      groupby = 1;
      hours = days;
      playerDataEndpoint = "playerdata1d";
    }

    const serverPlayerDataRequest = await fetch(
      `https://api.uncore.app/v1/servers/${playerDataEndpoint}/${serverid}?hours=${hours}${
        groupby >= 1 ? "&groupby=" + groupby : ""
      }`
    );
    const playerDataResponse: ServerPlayerDataResponse =
      await serverPlayerDataRequest.json();
    if (playerDataResponse.data) {
      setPlayerData(playerDataResponse);
    }
  };
  const handleUptimeDataTimeRangeChange = async (event: SelectChangeEvent) => {
    let days = Number.parseInt(event.target.value);
    setUptimeDataTimeRange(days);
    loadUptimeData(days);
  };
  const loadUptimeData = async (days: number) => {
    setUptimeData({ data: [] });
    let hours = days * 24;
    let groupby: number = 0.5;
    let uptimeDataEndpoint = "uptimedata";
    if (days == -1) {
      days = 500;
      hours = days * 24;
    }
    if (hours > 96) {
      groupby = 1;
      hours = days;
      uptimeDataEndpoint = "uptimedata1d";
    }

    const serverUptimeDataRequest = await fetch(
      `https://api.uncore.app/v1/servers/${uptimeDataEndpoint}/${serverid}?hours=${hours}${
        groupby >= 1 ? "&groupby=" + groupby : ""
      }`
    );
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
              <dd>{new Date(data.lastCheck).toLocaleString()}</dd>
              <dt>Found At: </dt>
              <dd>{new Date(data.foundAt).toLocaleString()}</dd>
              <dt>Next Check </dt>
              <dd>{new Date(data.nextCheck).toLocaleString()}</dd>
              <dt>ISP: </dt>
              <dd>{data.isp}</dd>
              <dt>ASN: </dt>
              <dd>{data.asn}</dd>
              <dt>Server Is Dead (not responding for 24 hours):</dt>
              <dd>{data.isOnline ? "Alive" : "Dead"}</dd>
              <dt>Unsuccessful Retries (failed to respond x checks in a row):</dt>
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
                <MenuItem value={3}>3 Days</MenuItem>
                <MenuItem value={30}>1 Month</MenuItem>
                <MenuItem value={365}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart
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
                data={playerData?.data?.map((d: ClickhousePlayerData) => {
                  return {
                    x: new Date(d.averageTime),
                    y: SimpleRound(d.playerAvg),
                  };
                })}
              />
            </VictoryChart>
          </Item>
        </Grid>
        <Grid xs md={6} mdOffset={0} padding={3}>
          <Item className={styles.serverPropertyList}>
            <dl>
              <dt>Ping from popular locations: </dt>
              <dd>
                Coming Soon
              </dd>
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
                <MenuItem value={3}>3 Days</MenuItem>
                <MenuItem value={30}>1 Month</MenuItem>
                <MenuItem value={365}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart domainPadding={{ y: 10 }}
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
                style={{ labels: {}, data: { stroke: "skyblue" } }}
                data={uptimeData?.data?.map((d: ClickHouseUptimeData) => {
                  return {
                    x: new Date(d.averageTime),
                    y: SimpleRound(d.uptime, 2),
                    ping: d.pingCount,
                    online: d.onlineCount
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
