// Types for the Uncore Metrics REST API (https://api.uncore.app).
// Property names follow ASP.NET's default camelCase, e.g. ServerID -> serverID, ISP -> isp.

/** Every successful response wraps its payload in `data`. */
export interface DataResponse<T> {
  data: T | null;
}

/** Every error response carries an `error` object. Note `Message` is capitalized by the API. */
export interface ErrorResponse {
  error: {
    code: number;
    Message: string;
    type?: string;
  };
}

export interface PagedResult<T> {
  currentPage: number;
  pageCount: number;
  pageSize: number;
  rowCount: number;
  results: T[];
}

export interface Game {
  appId: number;
  name: string;
  slugName: string;
}

/** A row in the server list and search results. */
export interface ServerSummary {
  serverID: string;
  name: string;
  game: string;
  map: string;
  appID: number;
  players: number;
  maxPlayers: number;
  isp: string | null;
  country: string | null;
}

export interface ServerPing {
  serverId: string;
  locationID: number;
  pingMs: number;
  failed: boolean;
  lastCheck: string;
  nextCheck: string;
  failedChecks: number;
}

export interface Server {
  serverID: string;
  name: string;
  game: string;
  map: string;
  appID: number;
  ipAddress: string;
  port: number;
  queryPort: number;
  players: number;
  maxPlayers: number;
  visibility: boolean | null;
  /** 'l' Linux, 'w' Windows, 'm' Mac, 'e' unknown */
  environment: string | null;
  vac: boolean | null;
  keywords: string | null;
  steamID: number | null;
  asn: number | null;
  isp: string | null;
  latitude: number | null;
  longitude: number | null;
  country: string | null;
  continent: string | null;
  timezone: string | null;
  isOnline: boolean;
  serverDead: boolean;
  lastCheck: string;
  nextCheck: string;
  failedChecks: number;
  foundAt: string;
  serverPings: ServerPing[] | null;
}

/** Game-specific fields (e.g. Rust's world size), as a flat property bag. */
export type ServerGameData = Record<string, string | number | boolean | string[] | number[] | null>;

export interface PingLocation {
  locationID: number;
  locationName: string;
  isp: string;
  asn: string;
  latitude: number;
  longitude: number;
  country: string;
}

export interface ScrapeJob {
  gameType: string;
  runType: string;
  node: string;
  internalId: string;
  runId: number;
  progress: number;
  totalDone: number;
  totalCount: number;
  runGuid: string;
  running: boolean;
  startedAt: string;
  lastUpdateUtc: string;
}

export interface OverallAnalytics {
  serverAnalyticsLast24H: number;
}

/** Grouped uptime from ClickHouse (per server, or overall when serverId is empty). */
export interface UptimeData {
  serverId: string;
  appId: number;
  uptime: number;
  pingCount: number;
  onlineCount: number;
  averageTime: string;
}

export interface RawUptimeData {
  serverId: string;
  isOnline: boolean;
  checkTime: string;
}

/** Grouped player counts from ClickHouse. The overall endpoints only fill playersMax. */
export interface PlayerData {
  serverId: string;
  appId: number;
  playerAvg: number;
  playersMin: number;
  playersMax: number;
  averageTime: string;
}

export interface RawPlayerData {
  serverId: string;
  players: number;
  checkTime: string;
}
