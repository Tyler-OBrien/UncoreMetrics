# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this project.

## What This Project Does

The Uncore Metrics website: live status, player counts, uptime history and ping results for Steam game servers. It
reads the UncoreMetrics REST API (`Web_Services/API`, deployed at `https://api.uncore.app`). It replaces the older
client-side Next.js site in `../uncore-metrics`, which is still in the repo.

## Commands

All commands run from this directory using pnpm:

```bash
pnpm install          # Install dependencies
pnpm dev              # Dev server at localhost:4321 (runs as a daemon: `pnpm astro dev stop` to stop it)
pnpm build            # Type-check (astro check) + build to ./dist
pnpm preview          # Preview the production build locally
```

Deploy with `pnpm build && pnpm exec wrangler deploy`. It's served at `uncore.app/*` via a worker route.

## Architecture

**Stack:** Astro 7 (SSR, `output: "server"`) + Preact (React compat) + Tailwind CSS 4, deployed to Cloudflare Workers
with `@astrojs/cloudflare`. The layout, theme, nav and chart theme were copied from the Action-Delay-API website and
rebranded.

**Data flow:**
- Pages fetch from the API on the server at render time through `src/lib/api.ts`. Its helpers return
  `{ data, error }`; pages show `ErrorState` instead of throwing.
- Browser-side refreshes (live status, jobs, chart range changes) go through `src/pages/api/[...path].ts`, a GET-only
  proxy with an allowlist of API paths. The browser never calls the API directly, so the API needs no CORS for this site.
  Add a path to `ALLOWED` before an island can use it.
- The API base URL is the `UNCORE_API_URL` var in `wrangler.toml`. In dev, `.env` wins (see `.env.example`), because
  wrangler's vars are loaded by `astro dev` too.
- Responses that aren't JSON count as failures. `uptimedataoverall` is deliberately hidden behind Cloudflare Access on
  api.uncore.app because the query is very expensive (it returns an HTML login page), so the site doesn't use it and the
  proxy doesn't allow it. Don't add it back.

**API shapes:** `models/api.ts`. The API uses ASP.NET's default camelCase, so `ServerID` is `serverID`, `ISP` is `isp`
and `AppID` is `appID`. Error bodies capitalize `Message`.

**Charts (`src/components/graph/`):**
- `TimeSeriesGraph.jsx`: Plotly line chart for uptime or players, for one server (`scope={{ serverId }}`) or overall
  (`scope={{ appId }}`, `appId: null` for all games). Always `client:only="preact"`.
- `src/lib/timeseries.ts` picks the endpoint and bucket size per range (raw ≤ 6h, 30-minute buckets ≤ 72h, 2h buckets
  for 7 days, daily beyond) to stay under the API's 500-result limit. Never send `groupby=0`, and hourly endpoints
  reject `groupby >= 24`.
- `TimeSeriesGraphLazy.jsx` waits for its parent `<details>` to open before loading.
- `chartTheme.js` is shared Plotly styling; charts re-theme on the `themeChange` event from `ThemeToggle.astro`.

**Live islands** (`client:load`, server-rendered with initial data, then polling the proxy):
`LiveServerStatus.jsx` (30s) and `JobsLive.jsx` (5s). The API has no SignalR, unlike Action-Delay-API.

**Games:** the list comes from the API's `v1/games` (`StaticGameInfo` in UncoreMetrics.Data). `src/data/games.ts` is
a fallback for an API without it. `getGames()` also reports `fromApi`. Only trust the `v1/servers?appId=` filter when
it's true, since an older API ignores the filter and returns every game's servers.

**Quirks in the data:**
- Rust and some other games overflow A2S_Info's one-byte max player count, so `players > maxPlayers` happens.
  Use `formatPlayers()`, which drops the meaningless maximum.
- Dead servers have `nextCheck` set to `DateTime.MaxValue`; check with `isRealDate()`.

**Styles:** `src/styles/global.css` (from the reference). Its tokens are `--fs-*`, `--elevation-*`, `--gray-*`,
`--accent-*`, `--status-*` and `--surface-*`. Don't name variables `--text-*`, `--shadow-*` or `--color-*`, because
Tailwind 4 reads those for its utilities. Site-specific classes (`data-table`, `fact-grid`, `text-input`,
`graph-select`, `progress`, `muted`) are at the end of the file.

## Pages

- `/`: games grid, 24h check count, search box
- `/servers`: paged list, `?q=` search, `?dead=1` to include dead servers
- `/servers/[id]`: live status, player and uptime charts, details, game-specific fields, ping per location
- `/games/[slug]`: one game's players chart and server list
- `/stats`: total players over time, all games or `?game=<appId>`
- `/locations`: ping locations grouped by country, with a text filter
- `/jobs`: collector scrape jobs with live progress
- `/about`, `/404`
