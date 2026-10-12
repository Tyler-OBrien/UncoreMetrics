import { describe, expect, it } from "vitest";
import { isAllowedApiPath } from "./proxy";

const ID = "b43ae65f-b447-448f-a183-2facf485b5cb";

describe("isAllowedApiPath", () => {
  it.each([
    `v1/servers/${ID}`,
    `v1/servers/uptimedataraw/${ID}`,
    `v1/servers/playerdata1d/${ID}`,
    `v2/servers/uptimedata/${ID}`,
    "v1/servers/playerdataoverall",
    "v1/servers/playerdataoverall/252490",
    "v1/jobs",
  ])("allows %s", (path) => {
    expect(isAllowedApiPath(path)).toBe(true);
  });

  it.each([
    // Deliberately hidden on the API because it's very expensive
    "v1/servers/uptimedataoverall",
    "v1/servers/uptimedataoverall/252490",
    // Other API routes aren't for the browser
    "v1/servers",
    "v1/servers/search/rust",
    "v2/servers/OverallAnalytics",
    "v1/locations",
    // Malformed ids and path tricks
    "v1/servers/not-a-guid",
    `v1/servers/${ID}/../../jobs`,
    `v1/servers/${ID}%2F..%2Fjobs`,
    `/v1/jobs`,
    "v1/jobs/",
    "",
  ])("rejects %s", (path) => {
    expect(isAllowedApiPath(path)).toBe(false);
  });
});
