import { describe, it, expect } from "vitest";
import { extractMarkdownTable } from "../utils/tableExtractor";

describe("extractMarkdownTable", () => {
  it("extracts markdown tables from assistant answers", () => {
    const table = extractMarkdownTable(`
| Metric | Value |
| --- | --- |
| Revenue | $4.2B |
| Margin | 23% |
`);

    expect(table.headers).toEqual(["Metric", "Value"]);
    expect(table.rows).toHaveLength(2);
    expect(table.rows[0][0]).toBe("Revenue");
  });
});
