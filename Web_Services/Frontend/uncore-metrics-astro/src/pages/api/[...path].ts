// Read-only proxy to the Uncore Metrics API for the browser islands (live refreshes, chart range changes).
// Only the GET paths allowlisted in lib/proxy.ts are forwarded; everything else is a 404, so this can't be used to reach other API routes.

import type { APIRoute } from "astro";
import { apiBase } from "../../lib/api";
import { isAllowedApiPath } from "../../lib/proxy";

// Short cache: these are live-ish numbers, but many viewers of one server shouldn't each hit the API.
const CACHE_SECONDS = 15;

export const GET: APIRoute = async ({ params, url }) => {
  const path = params.path ?? "";
  if (!isAllowedApiPath(path)) {
    return Response.json({ error: { code: 404, Message: "Not found", type: "not_found" } }, { status: 404 });
  }

  let upstream: Response;
  try {
    upstream = await fetch(`${apiBase()}/${path}${url.search}`, { headers: { accept: "application/json" } });
  } catch {
    return Response.json({ error: { code: 502, Message: "Couldn't reach the API", type: "bad_gateway" } }, { status: 502 });
  }

  // Anything but JSON (e.g. a Cloudflare Access login page) is an upstream failure, not data
  if (!(upstream.headers.get("content-type") ?? "").includes("json")) {
    return Response.json(
      { error: { code: 502, Message: "The API returned an unexpected response", type: "bad_gateway" } },
      { status: 502 }
    );
  }

  return new Response(upstream.body, {
    status: upstream.status,
    headers: {
      "content-type": upstream.headers.get("content-type") ?? "application/json",
      "cache-control": upstream.ok ? `public, max-age=${CACHE_SECONDS}` : "no-store",
    },
  });
};
