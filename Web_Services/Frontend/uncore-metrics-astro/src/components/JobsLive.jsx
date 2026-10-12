// Scrape job progress, refreshed every 5 seconds. Server-rendered with the initial list (client:load).

import { useEffect, useState } from "react";
import { formatNumber, relativeTime } from "../lib/format";

const REFRESH_MS = 5_000;

const sortJobs = (jobs) =>
  [...jobs].sort((a, b) => Number(b.running) - Number(a.running) || a.gameType.localeCompare(b.gameType) || a.runType.localeCompare(b.runType));

const JobsLive = ({ initial }) => {
  const [jobs, setJobs] = useState(() => sortJobs(initial));
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        const response = await fetch("/api/v1/jobs");
        if (response.ok) {
          const json = await response.json();
          if (Array.isArray(json.data)) setJobs(sortJobs(json.data));
        }
      } catch {
        // Keep the last good list
      }
      setNow(new Date());
    }, REFRESH_MS);
    return () => clearInterval(interval);
  }, []);

  if (jobs.length === 0) return <p className="muted">No scrape jobs have run yet.</p>;

  return (
    <ul className="grid gap-4 md:grid-cols-2 list-none p-0 m-0">
      {jobs.map((job) => (
        <li key={job.internalId} className="surface p-5 stack gap-3">
          <div className="flex items-start justify-between gap-3">
            <div className="stack gap-1">
              <span className="text-lg font-semibold">
                {job.gameType} {job.runType}
              </span>
              <span className="muted text-sm">
                {job.node} · run #{job.runId}
              </span>
            </div>
            <span className={`status-chip status-chip-${job.running ? "good" : "elevated"}`}>
              <span className="chip-dot" />
              {job.running ? "Running" : "Idle"}
            </span>
          </div>
          <div
            className="progress"
            role="progressbar"
            aria-valuenow={job.progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`${job.gameType} ${job.runType} progress`}
          >
            <span style={{ width: `${Math.min(100, Math.max(0, job.progress))}%` }} />
          </div>
          <div className="flex justify-between gap-3 text-sm muted tabular-nums">
            <span>
              {formatNumber(job.totalDone)} / {formatNumber(job.totalCount)} servers ({job.progress}%)
            </span>
            <span title={job.lastUpdateUtc}>updated {relativeTime(job.lastUpdateUtc, now)}</span>
          </div>
        </li>
      ))}
    </ul>
  );
};

export default JobsLive;
