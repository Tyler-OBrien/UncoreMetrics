// Picks the right API endpoint for a chart's time range, and turns the response into plot series.
// Kept free of Plotly/DOM so it can be reasoned about (and tested) on its own.

export type Kind = "uptime" | "players";

/** A single server's history, or overall history (appId null = every game). */
export type Scope = { serverId: string } | { appId: number | null };

export interface RangeOption {
  hours: number;
  label: string;
}

const ALL_RANGES: RangeOption[] = [
  { hours: 1, label: "Last hour" },
  { hours: 6, label: "Last 6 hours" },
  { hours: 24, label: "Last 24 hours" },
  { hours: 72, label: "Last 3 days" },
  { hours: 168, label: "Last 7 days" },
  { hours: 720, label: "Last 30 days" },
  { hours: 2160, label: "Last 90 days" },
];

/** Overall data only exists in 30 minute buckets and up, so short ranges would be one or two points. */
export function rangesFor(scope: Scope): RangeOption[] {
  return "serverId" in scope ? ALL_RANGES : ALL_RANGES.filter((range) => range.hours >= 24);
}

const KIND_PATH: Record<Kind, string> = { uptime: "uptime", players: "player" };

/**
 * The API path (relative to this site's /api proxy) for a range. Bucket sizes are chosen to stay under the
 * API's 500-result limit, and groupby is never 0 or ≥ 24 on the hourly endpoints, which the API rejects.
 */
export function endpointFor(kind: Kind, scope: Scope, hours: number): string {
  const k = KIND_PATH[kind];

  if ("serverId" in scope) {
    const id = encodeURIComponent(scope.serverId);
    // Every individual check: a few hundred rows at most for short ranges
    if (hours <= 6) return `v1/servers/${k}dataraw/${id}?hours=${hours}`;
    // 30 minute buckets
    if (hours <= 72) return `v2/servers/${k}data/${id}?hours=${hours}`;
    if (hours <= 168) return `v2/servers/${k}data/${id}?hours=${hours}&groupby=2`;
    // Daily buckets
    return `v1/servers/${k}data1d/${id}?days=${Math.round(hours / 24)}`;
  }

  const game = scope.appId ? `/${scope.appId}` : "";
  const base = `v1/servers/${k}dataoverall${game}?hours=${hours}`;
  if (hours <= 72) return base;
  if (hours <= 168) return `${base}&groupby=2`;
  if (hours <= 720) return `${base}&groupby=6`;
  return `${base}&groupby=24`;
}

export interface Series {
  name: string;
  x: string[];
  y: number[];
  /** Step lines for raw online/offline checks, smooth lines otherwise. */
  step?: boolean;
  dash?: boolean;
}

type Row = Record<string, unknown>;

const byTime = (timeKey: string) => (a: Row, b: Row) =>
  new Date(a[timeKey] as string).getTime() - new Date(b[timeKey] as string).getTime();

function groupByGame(rows: Row[], gameNames: Record<number, string>, value: (row: Row) => number): Series[] {
  const groups = new Map<number, Row[]>();
  for (const row of rows) {
    const appId = Number(row.appId ?? 0);
    groups.set(appId, [...(groups.get(appId) ?? []), row]);
  }
  return [...groups.entries()]
    .map(([appId, gameRows]) => ({
      // appId 0 is the all-games total the API returns when no game is chosen
      name: appId === 0 ? "All games" : gameNames[appId] ?? `App ${appId}`,
      x: gameRows.map((row) => row.averageTime as string),
      y: gameRows.map(value),
    }))
    .sort((a, b) => a.name.localeCompare(b.name));
}

/** Converts any of the uptime/player responses into chart series, oldest point first. */
export function toSeries(kind: Kind, scope: Scope, rows: Row[], gameNames: Record<number, string> = {}): Series[] {
  if (!rows.length) return [];

  // Raw checks have checkTime, bucketed data has averageTime
  if ("checkTime" in rows[0]) {
    const sorted = [...rows].sort(byTime("checkTime"));
    const x = sorted.map((row) => row.checkTime as string);
    return kind === "uptime"
      ? [{ name: "Online", x, y: sorted.map((row) => (row.isOnline ? 100 : 0)), step: true }]
      : [{ name: "Players", x, y: sorted.map((row) => Number(row.players)) }];
  }

  const sorted = [...rows].sort(byTime("averageTime"));

  if ("serverId" in scope) {
    const x = sorted.map((row) => row.averageTime as string);
    return kind === "uptime"
      ? [{ name: "Uptime", x, y: sorted.map((row) => Number(row.uptime)) }]
      : [
          { name: "Average", x, y: sorted.map((row) => Number(row.playerAvg)) },
          { name: "Peak", x, y: sorted.map((row) => Number(row.playersMax)), dash: true },
        ];
  }

  // Overall data: one line per game (only one when a game was chosen)
  return kind === "uptime"
    ? groupByGame(sorted, gameNames, (row) => Number(row.uptime))
    : groupByGame(sorted, gameNames, (row) => Number(row.playersMax));
}
