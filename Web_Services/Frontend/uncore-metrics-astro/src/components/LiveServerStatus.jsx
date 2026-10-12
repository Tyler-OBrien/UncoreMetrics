// Status, player count and check times for a server, refreshed every 30 seconds through the /api proxy.
// Rendered on the server with the initial data (client:load), so it's correct before any JavaScript runs.

import { useEffect, useState } from "react";
import { formatPlayers, isRealDate, relativeTime } from "../lib/format";

const REFRESH_MS = 30_000;

const LiveServerStatus = ({ initial }) => {
  const [server, setServer] = useState(initial);
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const refresh = async () => {
      try {
        const response = await fetch(`/api/v1/servers/${initial.serverID}`);
        if (!response.ok) return;
        const json = await response.json();
        if (json.data) setServer(json.data);
      } catch {
        // Keep showing the last good data
      }
      setNow(new Date());
    };
    const interval = setInterval(refresh, REFRESH_MS);
    // Also tick the relative times between refreshes
    const clock = setInterval(() => setNow(new Date()), 5_000);
    return () => {
      clearInterval(interval);
      clearInterval(clock);
    };
  }, []);

  const [label, tone] = server.serverDead
    ? ["Dead", "bad"]
    : server.isOnline
      ? ["Online", "good"]
      : ["Offline", "elevated"];

  return (
    <dl className="fact-grid">
      <div>
        <dt>Status</dt>
        <dd>
          <span className={`status-chip status-chip-${tone}`}>
            <span className="chip-dot" />
            {label}
          </span>
        </dd>
      </div>
      <div>
        <dt>Players</dt>
        <dd className="tabular-nums">{formatPlayers(server.players, server.maxPlayers)}</dd>
      </div>
      <div>
        <dt>Map</dt>
        <dd>{server.map || "—"}</dd>
      </div>
      <div>
        <dt>Last check</dt>
        <dd title={server.lastCheck}>{relativeTime(server.lastCheck, now)}</dd>
      </div>
      <div>
        <dt>Next check</dt>
        <dd title={server.nextCheck}>{isRealDate(server.nextCheck) ? relativeTime(server.nextCheck, now) : "Not scheduled"}</dd>
      </div>
      {server.failedChecks > 0 && (
        <div>
          <dt>Failed checks in a row</dt>
          <dd>{server.failedChecks}</dd>
        </div>
      )}
    </dl>
  );
};

export default LiveServerStatus;
