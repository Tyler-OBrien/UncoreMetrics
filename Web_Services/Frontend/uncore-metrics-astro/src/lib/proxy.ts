// Which API paths the browser may reach through /api/[...path]. Kept free of runtime imports so it can be unit tested.

const GUID = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

const ALLOWED: RegExp[] = [
  new RegExp(`^v1/servers/${GUID}$`),
  new RegExp(`^v1/servers/(uptimedataraw|uptimedata|uptimedata1d|playerdataraw|playerdata|playerdata1d)/${GUID}$`),
  // uptimedataoverall is left out on purpose: it's very expensive, so api.uncore.app keeps it behind Cloudflare Access
  /^v1\/servers\/playerdataoverall(\/\d+)?$/,
  new RegExp(`^v2/servers/(uptimedata|playerdata)/${GUID}$`),
  /^v1\/jobs$/,
];

/** True when `path` (no leading slash, no query string) may be forwarded to the API. */
export function isAllowedApiPath(path: string): boolean {
  return ALLOWED.some((pattern) => pattern.test(path));
}
