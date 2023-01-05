import { useRouter } from "next/router";
import * as React from "react";
import { styled } from "@mui/material/styles";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Unstable_Grid2";
import { useEffect } from "react";
import {
  ClickHousePlayerData,
  ClickHouseUptimeData,
  ServerPlayerDataResponse,
  ServerUptimeDataResponse,
  SingleServerResponse,
} from "../../interfaces/server";
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

const Server = () => {
  const router = useRouter();
  let { game } = router.query;
  let gameStr = game as string;
  if (!gameStr) {
    gameStr = "0";
  }

  const [selectedGame, setSelectedGame] = React.useState<string>(gameStr);
  if (!selectedGame && gameStr) {
    setSelectedGame(gameStr);
  }

  // @ts-ignore
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

  // https://stackoverflow.com/questions/11832914/how-to-round-to-at-most-2-decimal-places-if-necessary
  function SimpleRound(num: number, decimalPlaces = 0) {
    // @ts-ignore
    num = Math.round(num + "e" + decimalPlaces);
    return Number(num + "e" + -decimalPlaces);
  }
  const handlePlayerDataTimeRangeChange = async (event: SelectChangeEvent) => {
    let days = Number.parseInt(event.target.value);
    setPlayerDataTimeRange(days);
    loadPlayerData(selectedGame, days);
  };
  const loadPlayerData = async (game: string, days: number) => {
    if (game == "0") game = "";
    setPlayerData({ data: [] });
    let hours = days * 24;
    if (days == 0) {
      hours = 12;
    }
    let serverPlayerDataRequest;
    if (days == -1) {
      days = MAGIC_NUMBER_MAX_RESULTS - 1;
      hours = days * 24;
    }
    let groupby: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
    if (hours <= 24) {
      groupby = 0;
    }
    serverPlayerDataRequest = await fetch(
      `https://api.uncore.app/v1/servers/playerdataoverall/${game}?hours=${hours}${
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
    loadUptimeData(selectedGame, days);
  };
  const loadUptimeData = async (game: string, days: number) => {
    if (game == "0") game = "";
    setUptimeData({ data: [] });
    let hours = days * 24;
    if (days == 0) {
      hours = 12;
    }
    let serverUptimeDataRequest;
    if (days == -1) {
      days = MAGIC_NUMBER_MAX_RESULTS - 1;
      hours = days * 24;
    }
    let groupby: number = Math.ceil(hours / MAGIC_NUMBER_MAX_RESULTS);
    if (hours <= 24) {
      groupby = 0;
    }
    serverUptimeDataRequest = await fetch(
      `https://api.uncore.app/v1/servers/uptimedataoverall/${game}?hours=${hours}${
        groupby >= 1 ? "&groupby=" + groupby : ""
      }`
    );
    const uptimeDataResponse: ServerUptimeDataResponse =
      await serverUptimeDataRequest.json();
    if (uptimeDataResponse.data) {
      setUptimeData(uptimeDataResponse);
    }
  };
  const initialLoad = async (game: string) => {
    var initLoadPlayerDataPromise = loadPlayerData(game, playerDataTimeRange);
    var initLoadUptimeDataPromise = loadUptimeData(game, uptimeDataTimeRange);
    await Promise.all([initLoadPlayerDataPromise, initLoadUptimeDataPromise]);
  };

  if (loading && !error && selectedGame) {
    setLoading(false);
    initialLoad(selectedGame);
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
        <Grid xs={6}>
          <FormControl size="medium" style={{ width: "50%" }}>
            <InputLabel id="game-type-select-label">Game</InputLabel>
            <Select
              labelId="game-type-select-label"
              id="game-type-select"
              label="Game"
              onChange={(event: SelectChangeEvent) => {
                router.push({
                  pathname: "/stats",
                  query: { game: event.target.value },
                });
                setSelectedGame(event.target.value.toString());
                initialLoad(event.target.value.toString());
              }}
              value={selectedGame}
            >
              <MenuItem value={"0"}>All</MenuItem>
              <MenuItem value={"346110"}>ARK</MenuItem>
              <MenuItem value={"107410"}>Arma 3</MenuItem>
              <MenuItem value={"221100"}>DayZ</MenuItem>
              <MenuItem value={"686810"}>Hell Let Loose</MenuItem>
              <MenuItem value={"736220"}>Post Scriptum</MenuItem>
              <MenuItem value={"108600"}>Project Zomboid</MenuItem>
              <MenuItem value={"252490"}>Rust</MenuItem>
              <MenuItem value={"251570"}>7 Days to Die</MenuItem>
              <MenuItem value={"304930"}>Unturned</MenuItem>
              <MenuItem value={"1604030"}>V Rising</MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>
      <Grid container spacing={3} padding={3}></Grid>
      <Grid container spacing={3} sx={{ flexGrow: 1 }}>
        <Grid xs={6} mdOffset="auto">
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
                <MenuItem value={0}>12 hours</MenuItem>
                <MenuItem value={1}>1 Day</MenuItem>
                <MenuItem value={3}>3 Days</MenuItem>
                <MenuItem value={14}>14 Days</MenuItem>
                <MenuItem value={30}>1 Month</MenuItem>
                <MenuItem value={90}>3 Months</MenuItem>
                <MenuItem value={180}>6 Months</MenuItem>
                <MenuItem value={365}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart
              domainPadding={{ y: 20, x: 5 }}
              scale={{ x: "time" }}
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
                data={playerData?.data?.map((d: ClickHousePlayerData) => {
                  return {
                    x: new Date(d.averageTime),
                    y: SimpleRound(d.playersMax),
                  };
                })}
              />
            </VictoryChart>
          </Item>
        </Grid>
        <Grid xs={6} mdOffset="auto">
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
                <MenuItem value={0}>12 hours</MenuItem>
                <MenuItem value={1}>1 Day</MenuItem>
                <MenuItem value={3}>3 Days</MenuItem>
                <MenuItem value={14}>14 Days</MenuItem>
                <MenuItem value={30}>1 Month</MenuItem>
                <MenuItem value={90}>3 Months</MenuItem>
                <MenuItem value={180}>6 Months</MenuItem>
                <MenuItem value={365}>1 Year</MenuItem>
                <MenuItem value={-1}>Max</MenuItem>
              </Select>
            </FormControl>
            <VictoryChart
              domainPadding={{ y: 20, x: 5 }}
              scale={{ x: "time" }}
              containerComponent={
                <VictoryVoronoiContainer
                  labels={({ datum }) =>
                    `${new Date(datum.x).toLocaleString()}\n${
                      datum.y
                    }% successful checks\n ${datum?.online?.toLocaleString()}/${datum?.ping?.toLocaleString()}`
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
                    online: d.onlineCount,
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
