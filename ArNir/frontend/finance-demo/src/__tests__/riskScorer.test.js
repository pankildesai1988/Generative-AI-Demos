import { describe, it, expect } from "vitest";
import { scoreRisk } from "../utils/riskScorer";

describe("scoreRisk", () => {
  it("returns a moderate or high score when multiple risk factors exist", () => {
    const result = scoreRisk(
      "Debt, volatility, uncertainty and regulatory risk remain material concerns."
    );

    expect(result.score).toBeGreaterThan(20);
    expect(result.factors.length).toBeGreaterThan(1);
  });
});
