import { describe, expect, it } from "vitest";
import { endpointFor, rangesFor, toSeries, type Scope } from "./timeseries";

const SERVER: Scope = { serverId: "b43ae65f-b447-448f-a183-2facf485b5cb" };
const ALL_GAMES: Scope = { appId: null };
const RUST: Scope = { appId: 252490 };

function query(path: string): URLSearchParams {
  return new URLSearchParams(path.split("?")[1] ?? "");
}

/** Results the API would return for a path: it rejects anything over 500. */
function expectedResults(path: string): number {
  const q = query(path);
  if (q.has("days")) return Number(q.get("days")) / Number(q.get("groupby") ?? 1);
  const hours = Number(q.get("hours"));
  if (path.includes("dataraw")) return 0; // raw data is limited by row count on the API side, not bucketed
  return q.has("groupby") ? hours / Number(q.get("groupby")) : hours / 0.5;
}

describe("rangesFor", () => {
  it("offers short ranges only for a single server", () => {
    expect(rangesFor(SERVER).map((r) => r.hours)).toContain(1);
    expect(Math.min(...rangesFor(ALL_GAMES).map((r) => r.hours))).toBe(24);
  });
});

describe("endpointFor", () => {
  for (const kind of ["uptime", "players"] as const) {
    for (const scope of [SERVER, ALL_GAMES, RUST]) {
      for (const { hours } of rangesFor(scope)) {
        it(`${kind} ${JSON.stringify(scope)} ${hours}h stays within the API's limits`, () => {
          const path = endpointFor(kind, scope, hours);
          const q = query(path);
          expect(expectedResults(path)).toBeLessThanOrEqual(500);
          if (q.has("groupby")) {
            expect(Number(q.get("groupby"))).toBeGreaterThan(0);
            // Per-server hourly endpoints reject groupby >= 24 (use the 1d ones); the overall endpoints accept it
            if ("serverId" in scope && !path.includes("1d")) expect(Number(q.get("groupby"))).toBeLessThan(24);
          }
        });
      }
    }
  }

  it("uses raw checks for short server ranges", () => {
    expect(endpointFor("uptime", SERVER, 1)).toBe(`v1/servers/uptimedataraw/${(SERVER as { serverId: string }).serverId}?hours=1`);
  });

  it("uses daily buckets for long server ranges", () => {
    expect(endpointFor("players", SERVER, 720)).toMatch(/^v1\/servers\/playerdata1d\/.+\?days=30$/);
  });

  it("filters overall data by game only when one is chosen", () => {
    expect(endpointFor("players", ALL_GAMES, 24)).toBe("v1/servers/playerdataoverall?hours=24");
    expect(endpointFor("players", RUST, 24)).toBe("v1/servers/playerdataoverall/252490?hours=24");
  });
});

describe("toSeries", () => {
  it("returns nothing for no rows", () => {
    expect(toSeries("uptime", SERVER, [])).toEqual([]);
  });

  it("turns raw online checks into a 0/100 step line, oldest first", () => {
    const series = toSeries("uptime", SERVER, [
      { isOnline: false, checkTime: "2026-10-11T22:01:00" },
      { isOnline: true, checkTime: "2026-10-11T22:00:00" },
    ]);
    expect(series).toEqual([{ name: "Online", x: ["2026-10-11T22:00:00", "2026-10-11T22:01:00"], y: [100, 0], step: true }]);
  });

  it("splits grouped player data into average and peak", () => {
    const series = toSeries("players", SERVER, [{ averageTime: "2026-10-11T22:00:00", playerAvg: 10.5, playersMax: 20 }]);
    expect(series.map((s) => s.name)).toEqual(["Average", "Peak"]);
    expect(series[1]).toMatchObject({ y: [20], dash: true });
  });

  it("draws one line per game for overall data, labelling the all-games total", () => {
    const rows = [
      { appId: 252490, averageTime: "2026-10-11T22:00:00", playersMax: 100 },
      { appId: 0, averageTime: "2026-10-11T22:00:00", playersMax: 500 },
      { appId: 999, averageTime: "2026-10-11T22:00:00", playersMax: 5 },
    ];
    const names = toSeries("players", ALL_GAMES, rows, { 252490: "Rust" }).map((s) => s.name);
    expect(names).toEqual(["All games", "App 999", "Rust"]);
  });
});
