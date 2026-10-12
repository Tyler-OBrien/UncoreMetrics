// Server-side access to the Uncore Metrics REST API.
// Pages call these at render time. Browser-side refreshes go through /api/[...path] instead.

import { env } from "cloudflare:workers";
import type {
  DataResponse,
  ErrorResponse,
  Game,
  OverallAnalytics,
  PagedResult,
  PingLocation,
  ScrapeJob,
  Server,
  ServerGameData,
  ServerSummary,
} from "../../models/api";
import { FALLBACK_GAMES } from "../data/games";

/**
 * Base URL of the REST API. In dev a `.env` value wins, because wrangler's [vars] are loaded by `astro dev` too
 * and would otherwise make the production URL unconditional. Gated on DEV so it can't redirect a deployed Worker.
 */
export function apiBase(): string {
  const local = import.meta.env.UNCORE_API_URL;
  const configured = import.meta.env.DEV && local ? local : (env as Env | undefined)?.UNCORE_API_URL || local;
  return (configured || "https://api.uncore.app").replace(/\/$/, "");
}

/** Either the payload, or a message to show instead. Pages render the error rather than throwing. */
export type ApiResult<T> = { data: T; error: null } | { data: null; error: string };

export async function apiGet<T>(path: string): Promise<ApiResult<T>> {
  let response: Response;
  try {
    response = await fetch(`${apiBase()}/${path.replace(/^\//, "")}`, {
      headers: { accept: "application/json" },
    });
  } catch {
    return { data: null, error: "Couldn't reach the Uncore Metrics API." };
  }

  let body: Partial<DataResponse<T> & ErrorResponse>;
  try {
    body = await response.json();
  } catch {
    return { data: null, error: `The API returned an unexpected response (HTTP ${response.status}).` };
  }

  if (body.error) return { data: null, error: body.error.Message || `HTTP ${response.status}` };
  if (!response.ok) return { data: null, error: `HTTP ${response.status}` };
  if (body.data === null || body.data === undefined) return { data: null, error: "Not found." };
  return { data: body.data, error: null };
}

export const getServer = (id: string) => apiGet<Server>(`v1/servers/${encodeURIComponent(id)}`);

export const getServerGameData = (id: string, appId: number) =>
  apiGet<ServerGameData>(`v1/serverdata/${encodeURIComponent(id)}/${appId}`);

export function getServers(options: { page?: number; pageSize?: number; includeDead?: boolean; appId?: number }) {
  const query = new URLSearchParams({
    page: String(options.page ?? 1),
    pageSize: String(options.pageSize ?? 25),
    includeDead: String(options.includeDead ?? false),
  });
  if (options.appId) query.set("appId", String(options.appId));
  return apiGet<PagedResult<ServerSummary>>(`v1/servers?${query}`);
}

export function searchServers(search: string, options: { page?: number; pageSize?: number; includeDead?: boolean }) {
  const query = new URLSearchParams({
    page: String(options.page ?? 1),
    pageSize: String(options.pageSize ?? 25),
    includeDead: String(options.includeDead ?? false),
  });
  return apiGet<PagedResult<ServerSummary>>(`v1/servers/search/${encodeURIComponent(search)}?${query}`);
}

export const getLocations = () => apiGet<PingLocation[]>("v1/locations");

export const getJobs = () => apiGet<ScrapeJob[]>("v1/jobs");

export const getOverallAnalytics = () => apiGet<OverallAnalytics>("v2/servers/OverallAnalytics");

/** The game list, falling back to a built-in copy if the API doesn't have v1/games yet. */
export async function getGames(): Promise<Game[]> {
  const result = await apiGet<Game[]>("v1/games");
  return result.data?.length ? result.data : FALLBACK_GAMES;
}
