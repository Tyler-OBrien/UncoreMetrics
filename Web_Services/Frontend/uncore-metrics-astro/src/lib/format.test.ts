import { describe, expect, it } from "vitest";
import { environmentName, formatNumber, formatPlayers, isRealDate, labelFromKey, relativeTime } from "./format";

describe("formatPlayers", () => {
  it("shows players out of max", () => {
    expect(formatPlayers(12, 100)).toBe("12 / 100");
  });

  it("drops the max when it has overflowed (Rust reports e.g. 1100 / 7)", () => {
    expect(formatPlayers(1100, 7)).toBe("1,100");
  });
});

describe("isRealDate", () => {
  it("rejects DateTime.MaxValue, used for dead servers' next check", () => {
    expect(isRealDate("9999-12-31T23:59:59.9999999")).toBe(false);
  });

  it("accepts a normal date", () => {
    expect(isRealDate("2026-10-11T22:00:00Z")).toBe(true);
  });
});

describe("relativeTime", () => {
  const now = new Date("2026-10-11T22:00:00Z");

  it("describes past and future times", () => {
    expect(relativeTime("2026-10-11T21:57:00Z", now)).toBe("3 minutes ago");
    expect(relativeTime("2026-10-11T22:00:30Z", now)).toBe("in 30 seconds");
  });

  it("handles invalid input", () => {
    expect(relativeTime("not a date", now)).toBe("—");
  });
});

describe("formatting helpers", () => {
  it("formats missing numbers as a dash", () => {
    expect(formatNumber(null)).toBe("—");
    expect(formatNumber(1234.567, 1)).toBe("1,234.6");
  });

  it("names operating systems", () => {
    expect(environmentName("l")).toBe("Linux");
    expect(environmentName(null)).toBe("Unknown");
  });

  it("turns API property names into labels", () => {
    expect(labelFromKey("EntityCount")).toBe("Entity Count");
    expect(labelFromKey("world_size")).toBe("World size");
  });
});
