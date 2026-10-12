import type { Game } from "../../models/api";

/**
 * Used only when the API doesn't serve v1/games yet. The source of truth is StaticGameInfo in
 * Data_Services/UncoreMetrics.Data/GameData/StaticGameInfo.cs.
 */
export const FALLBACK_GAMES: Game[] = [
  { appId: 346110, name: "ARK", slugName: "ARK" },
  { appId: 107410, name: "Arma 3", slugName: "Arma3" },
  { appId: 221100, name: "DayZ", slugName: "DayZ" },
  { appId: 686810, name: "Hell Let Loose", slugName: "Hell-Let-Loose" },
  { appId: 736220, name: "Post Scriptum", slugName: "Post-Scriptum" },
  { appId: 108600, name: "Project Zomboid", slugName: "Project-Zomboid" },
  { appId: 252490, name: "Rust", slugName: "Rust" },
  { appId: 251570, name: "7 Days To Die", slugName: "7-days-to-die" },
  { appId: 393380, name: "Squad", slugName: "Squad" },
  { appId: 304930, name: "Unturned", slugName: "Unturned" },
  { appId: 1604030, name: "V Rising", slugName: "V-Rising" },
];

export function findGameBySlug(games: Game[], slug: string): Game | undefined {
  return games.find((game) => game.slugName.toLowerCase() === slug.toLowerCase());
}
