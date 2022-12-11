import { useRouter } from "next/router";
import * as React from "react";
import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Unstable_Grid2";
import { useEffect } from "react";
import { ClickhousePlayerData, ClickHouseUptimeData, ServerPlayerDataResponse, ServerUptimeDataResponse, SingleServerResponse } from "../../interfaces/server";
import { Server } from "../../interfaces/server";
import styles from "./[serverid].module.css";
import { VictoryChart, VictoryLine, VictoryZoomContainer } from "victory";

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
  const [uptimeData, setUptimeData] = React.useState<ServerUptimeDataResponse>(undefined); // @ts-ignore
  const [playerData, setPlayerData] = React.useState<ServerPlayerDataResponse>(undefined); // @ts-ignore
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string>("");

  useEffect(() => {
    const interval = setInterval(() => updateData(ServerID), 10000);
    return () => {
      clearInterval(interval);
    };
  }, [ServerID]);

  const updateData = async (serverid: string) => {



    const serverFetchRequest = fetch(
      `https://api.uncore.app/v1/servers/${serverid}`
    );
    const serverUptimeDataRequest = fetch(`https://api.uncore.app/v1/servers/uptimedata/${serverid}?hours=96&groupby=1`);
    
    const serverPlayerDataRequest = fetch(`https://api.uncore.app/v1/servers/playerdata/${serverid}?hours=96&groupby=1`);

    const serverDataResponse = await serverFetchRequest;
    const serverResponse: SingleServerResponse = await serverDataResponse.json();
    if (serverResponse.error) {
      setError(serverResponse.error.Message);
    }
    if (serverResponse.data) {
      setData(serverResponse?.data);
    }
    setLoading(false);
    const uptimeDataResponse: ServerUptimeDataResponse = await (await serverUptimeDataRequest).json();
    if (uptimeDataResponse.data) {
      setUptimeData(uptimeDataResponse);
    }
    const playerDataResponse: ServerPlayerDataResponse = await (await serverPlayerDataRequest).json();
    if (uptimeDataResponse.data) {
      setPlayerData(playerDataResponse);
    }
  };
  if (loading && !error && !data && serverid) {
    updateData(serverid as string);
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
              <dt>ISP: </dt>
              <dd>{data.isp}</dd>
              <dt>ASN: </dt>
              <dd>{data.asn}</dd>
            </dl>
          </Item>
        </Grid>
        <Grid xs={4} mdOffset="auto">
          <Item>
            <h2>Player Data</h2>
          <VictoryChart domainPadding={{ y: 10 }}
            containerComponent={
              <VictoryZoomContainer/>
            }
          >
            <VictoryLine
                  style={{

                  }}
                  data={playerData?.data?.map((d: ClickhousePlayerData) => { return { x: new Date(d.averageTime), y: d.playerAvg } }) }
            />
          </VictoryChart>
          </Item>
        </Grid>
        <Grid xs={4} mdOffset="auto">
          <Item>
          <h2>Uptime Data</h2>
          <VictoryChart domainPadding={{ y: 10 }}
            containerComponent={
              <VictoryZoomContainer/>
            }
          >
            <VictoryLine
                  style={{

                  }}
                  data={uptimeData?.data?.map((d: ClickHouseUptimeData) => { return { x: new Date(d.averageTime), y: d.uptime } }) }
            />
          </VictoryChart>
          </Item>
        </Grid>
      </Grid>
    </div>
  );
};

export default Server;
