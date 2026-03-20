import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import InsightsPanel from "../components/InsightsPanel";

describe("InsightsPanel", () => {
  it("shows empty state when no data", () => {
    render(<InsightsPanel answer="" hasData={false} />);
    expect(screen.getByText("Key Insights")).toBeInTheDocument();
    expect(
      screen.getByText("Financial insights will appear here after you ask a question.")
    ).toBeInTheDocument();
  });

  it("extracts dollar amounts from answer", () => {
    render(
      <InsightsPanel
        answer="Revenue was $4.2 billion with operating income of $966 million"
        hasData={true}
      />
    );
    expect(screen.getByText("Financial Figures")).toBeInTheDocument();
    expect(screen.getByText("$4.2 billion")).toBeInTheDocument();
    expect(screen.getByText("$966 million")).toBeInTheDocument();
  });

  it("detects risk keywords", () => {
    render(
      <InsightsPanel
        answer="Key risk factors include supply chain disruption and debt levels"
        hasData={true}
      />
    );
    expect(screen.getByText("Risk Flags")).toBeInTheDocument();
    expect(screen.getByText("risk detected")).toBeInTheDocument();
    expect(screen.getByText("debt detected")).toBeInTheDocument();
  });

  it("detects growth/positive indicators", () => {
    render(
      <InsightsPanel
        answer="Strong revenue growth driven by profit expansion"
        hasData={true}
      />
    );
    expect(screen.getByText("Positive Indicators")).toBeInTheDocument();
  });

  it("extracts percentages", () => {
    render(
      <InsightsPanel
        answer="Gross margin improved to 67% with a growth rate of 12%"
        hasData={true}
      />
    );
    expect(screen.getByText("Key Percentages")).toBeInTheDocument();
    expect(screen.getByText("67%")).toBeInTheDocument();
    expect(screen.getByText("12%")).toBeInTheDocument();
  });

  it("shows no-metrics message when answer has no financial data", () => {
    render(
      <InsightsPanel answer="Hello, how are you?" hasData={true} />
    );
    expect(
      screen.getByText("No specific financial metrics detected in the latest response.")
    ).toBeInTheDocument();
  });
});
