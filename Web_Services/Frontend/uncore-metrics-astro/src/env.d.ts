/// <reference path="../.astro/types.d.ts" />
/// <reference types="astro/client" />

/** Vars declared in wrangler.toml, reached through `import { env } from "cloudflare:workers"`. */
interface Env {
  /** The Uncore Metrics REST API, e.g. https://api.uncore.app */
  UNCORE_API_URL?: string;
}

interface ImportMetaEnv {
  /** Dev-only override from .env, see .env.example */
  readonly UNCORE_API_URL?: string;
}

/**
 * The Workers runtime provides this module. Its types live in @cloudflare/workers-types, which is only a
 * transitive dependency, so declare the one export used here.
 */
declare module "cloudflare:workers" {
  export const env: Env;
}
