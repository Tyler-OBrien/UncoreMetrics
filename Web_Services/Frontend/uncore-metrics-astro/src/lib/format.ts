// Small formatting helpers shared by pages and islands (no server-only imports here).

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
  ["year", 365 * 24 * 60 * 60 * 1000],
  ["month", 30 * 24 * 60 * 60 * 1000],
  ["day", 24 * 60 * 60 * 1000],
  ["hour", 60 * 60 * 1000],
  ["minute", 60 * 1000],
  ["second", 1000],
];

/** "3 minutes ago", "in 20 seconds". */
export function relativeTime(iso: string | Date, now: Date = new Date()): string {
  const date = typeof iso === "string" ? new Date(iso) : iso;
  if (Number.isNaN(date.getTime())) return "—";
  const elapsed = date.getTime() - now.getTime();
  const format = new Intl.RelativeTimeFormat("en", { numeric: "auto" });
  for (const [unit, ms] of UNITS) {
    if (Math.abs(elapsed) >= ms || unit === "second") return format.format(Math.round(elapsed / ms), unit);
  }
  return "—";
}

/** A far-future date (e.g. dead servers' NextCheck of DateTime.MaxValue) isn't worth showing. */
export function isRealDate(iso: string): boolean {
  const year = new Date(iso).getUTCFullYear();
  return year > 2000 && year < 9000;
}

export function formatNumber(value: number | null | undefined, maximumFractionDigits = 0): string {
  if (value === null || value === undefined || Number.isNaN(value)) return "—";
  return value.toLocaleString("en", { maximumFractionDigits });
}

export function formatPercent(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return "—";
  return `${value.toLocaleString("en", { maximumFractionDigits: 1 })}%`;
}

/**
 * "12 / 100". Some games (Rust) overflow A2S_Info's one-byte max player count, so a server can report more players
 * than its maximum; the maximum is meaningless then and left out.
 */
export function formatPlayers(players: number, maxPlayers: number): string {
  return players > maxPlayers ? formatNumber(players) : `${formatNumber(players)} / ${formatNumber(maxPlayers)}`;
}

const ENVIRONMENTS: Record<string, string> = { l: "Linux", w: "Windows", m: "Mac" };

export function environmentName(code: string | null | undefined): string {
  return (code && ENVIRONMENTS[code]) || "Unknown";
}

/** Turn API property names like "PvE" or "EntityCount" into labels. */
export function labelFromKey(key: string): string {
  return key
    .replace(/_/g, " ")
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/^./, (c) => c.toUpperCase());
}
