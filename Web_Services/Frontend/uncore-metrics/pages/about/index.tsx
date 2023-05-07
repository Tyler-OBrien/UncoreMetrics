import { Box, Link, List, ListItem, Paper, Typography } from "@mui/material";
import type { NextPage } from "next";
import Head from "next/head";

const About: NextPage = () => {
  return (
    <Box className="flex w-full items-center justify-center border-t" sx={{ width: "100%"}}>
      <Paper variant="outlined" square className="flex items-center justify-center gap-2" sx={{ width: "80%", mb: 2, }}>
      <div className="flex min-h-screen flex-col items-center justify-center py-2">
        <Head>
          <title>Uncore Metrics - About</title>
        </Head>

        <main className="w-6/12 items-center justify-center text-center">
          <Typography className="h2 text-xl">About Uncore Metrics</Typography>
          <Typography>Uncore Metrics makes queries to community steam game servers and collects various statistics about them, and displays them here for easy viewing. We try to query each server about once a minute. Server Pings are about once every 10 minutes.</Typography >
          <Typography className="h2 text-xl">Discord Bot</Typography>
          <Typography>Uncore Metrics has a Discord Bot, you can invite it via <Link href="https://discord.com/oauth2/authorize?client_id=1054223662124912700&permissions=415001504832&scope=bot">this link</Link>. You can use the /search or /server commands to look up servers and information about them. You can use /linkchannel to get notifications about a server going up/down in a specified channel.</Typography>
          <Typography className="h2 text-xl">Collection Policies</Typography>
          <Typography>Uncore Metrics discovers servers by the Steam Web API every 5 minutes, searching for servers of that game that contain players. Once a server is picked up by this, we will continue to poll it even if it has no players in it. If a server starts failing, we will schedule it with less frequency the more times it fails, eventually only checking once per hour and marking it as dead after 14 days of failure.</Typography>
          <Typography className="h2 text-xl">Open Source</Typography>
          <Typography>Uncore Metrics is fully <Link href="https://github.com/Tyler-OBrien/UncoreMetrics">open source</Link>, we use a <Link href="https://github.com/Tyler-OBrien/Okolni-Source-Query-Pool">modified Steam Server Query Library</Link> using a single UDP Socket to collect information via the Steam Server Query Protocol.</Typography>
          <Typography className="h2 text-xl">Infrastructure</Typography>
          <Typography>The main server runs a Ryzen 7700, 64GB RAM, 1tb NVMe and 12 tb HDD for storing long term statistics. We use Clickhouse to store the results of every poll, around 100 million a day/3 billion a month. We, however, do take advantage of Materialized Views for better query response time/less resource use.</Typography>
          <Typography className="h2 text-xl">Todo:</Typography>
          <List className="w-6/12" sx={{ listStyleType: 'disc' }}>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>Improve Mobile View of the website</ListItem>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>Add better graphs, you should be able to pan and zoom to see the more granular data we are already storing</ListItem>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>More Games</ListItem>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>More Server Ping Locations</ListItem>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>Make the server list sortable</ListItem>
          <ListItem className="items-center justify-center" sx={{ display: 'list-item' }}>Make the server list filterable</ListItem>
        </List>
         <Typography>You can email me at tyler @ this website.</Typography>
        </main>

        <footer className="flex h-24 w-full items-center justify-center border-t space-x-2">
          <a
            className="flex items-center justify-center gap-2"
            href="https://github.com/Tyler-OBrien/UncoreMetrics"
            target="_blank"
            rel="noopener noreferrer"
          >
            Powered by Uncore Metrics
          </a>
        </footer>
      </div>
      </Paper>
    </Box>
  );
};

export default About;
