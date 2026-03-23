import { describe, expect, it } from "vitest";
import { parseSseEventBlock } from "../api/ragStream";

describe("parseSseEventBlock", () => {
  it("parses token events with json payloads", () => {
    const parsed = parseSseEventBlock(
      'event: token\ndata: {"text":"hello "}\n'
    );

    expect(parsed).toEqual({
      event: "token",
      data: { text: "hello " },
    });
  });

  it("defaults to message events for plain text payloads", () => {
    const parsed = parseSseEventBlock("data: stream finished\n");

    expect(parsed).toEqual({
      event: "message",
      data: "stream finished",
    });
  });

  it("returns null for empty event blocks", () => {
    expect(parseSseEventBlock("\n\n")).toBeNull();
  });
});
