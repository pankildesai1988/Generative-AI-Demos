import { describe, it, expect } from "vitest";
import { extractHighlightTerms, getHighlightLabels } from "../utils/highlightTerms";

describe("highlightTerms", () => {
  it("extracts capitalized entity phrases", () => {
    const terms = extractHighlightTerms("Apple Inc launched a new iPhone in California.");
    const labels = terms.map((t) => t.label);
    expect(labels).toContain("Apple Inc");
    expect(labels).toContain("California");
  });

  it("extracts numeric quantities with units", () => {
    const terms = extractHighlightTerms("CPU at 95% used 512mb of RAM in 1500ms.");
    const labels = terms.map((t) => t.label);
    expect(labels).toContain("95%");
    expect(labels).toContain("512mb");
    expect(labels).toContain("1500ms");
  });

  it("extracts inline code spans", () => {
    const terms = extractHighlightTerms("Use `useEffect` to subscribe to `state`.");
    const codeTerms = terms.filter((t) => t.category === "code").map((t) => t.label);
    expect(codeTerms).toEqual(expect.arrayContaining(["useEffect", "state"]));
  });

  it("returns empty array for empty input", () => {
    expect(extractHighlightTerms("")).toEqual([]);
    expect(getHighlightLabels("")).toEqual([]);
  });
});
