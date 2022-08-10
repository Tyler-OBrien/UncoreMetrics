import React, { useEffect } from "react";
import { ScrapeJob, ScrapeJobResponse } from "../interfaces/scrapejobs";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Paper from "@mui/material/Paper";
import { Server } from "http";

function returnEntryRow(row: ScrapeJob) {
  return (
    <TableRow
      key={row.name}
      sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
    >
      <TableCell component="th" scope="row">
        {row.gameType}
      </TableCell>
      <TableCell align="right">{row.progress}%</TableCell>
      <TableCell align="right">{row.runType}</TableCell>
      <TableCell align="right">{row.totalCount}</TableCell>
      <TableCell align="right">{row.totalDone}</TableCell>
      <TableCell align="right">
        {new Date(row.startedAt).toLocaleTimeString()}
      </TableCell>
      <TableCell align="right">
        {new Date(row.lastUpdateUtc).toLocaleTimeString()}
      </TableCell>
      <TableCell align="right">{row.node}</TableCell>
      <TableCell align="right">{row.running.toString()}</TableCell>
    </TableRow>
  );
}

export default function ScrapeJobDisplay() {
  // @ts-ignore
  const [data, setData] = React.useState<ScrapeJob[]>(undefined);

  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string>("");

  useEffect(() => {
    const interval = setInterval(() => updateData(), 1000);
    return () => {
      clearInterval(interval);
    };
  }, []);

  const updateData = async () => {
    const response = await fetch(`https://api.uncore.app/v1/jobs`);
    const serverResponse: ScrapeJobResponse = await response.json();
    if (serverResponse.error) {
      setError(serverResponse.error.Message);
    }
    if (serverResponse.data) {
      setData(serverResponse?.data);
    }
    setLoading(false);
  };
  if (loading && !error && !data) {
    updateData();
  }
  if (loading) {
    return <div>Loading...</div>;
  }
  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} aria-label="simple table">
        <TableHead>
          <TableRow>
            <TableCell>Game</TableCell>
            <TableCell align="right">Progress</TableCell>
            <TableCell align="right">Run Type</TableCell>
            <TableCell align="right">Run Count</TableCell>
            <TableCell align="right">Run Done</TableCell>
            <TableCell align="right">Started At</TableCell>
            <TableCell align="right">Last Update</TableCell>
            <TableCell align="right">Node Name</TableCell>
            <TableCell align="right">Is Running</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {data
            .filter((scrapejob) => scrapejob.running)
            .sort(
              (a, b) =>
                new Date(b.lastUpdateUtc).getMilliseconds() -
                new Date(a.lastUpdateUtc).getMilliseconds()
            )
            .map((row) => returnEntryRow(row))}
          {data
            .filter((scrapejob) => !scrapejob.running)
            .sort(
              (a, b) =>
                new Date(b.lastUpdateUtc).getMilliseconds() -
                new Date(a.lastUpdateUtc).getMilliseconds()
            )
            .map((row) => returnEntryRow(row))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
