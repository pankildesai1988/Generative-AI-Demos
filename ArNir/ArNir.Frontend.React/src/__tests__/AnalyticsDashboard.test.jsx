import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import AnalyticsDashboard from "../components/analytics/AnalyticsDashboard";

vi.mock("../api/analytics", () => ({
  getAnalyticsOverview: vi.fn().mockResolvedValue({
    kpis: [
      { title: "SLA Compliance", value: "97.6%", trend: "+0.4%" },
      { title: "Avg Latency", value: "2,720 ms", trend: "-1.1%" },
    ],
    charts: [],
  }),
  exportAnalytics: vi.fn(),
}));

describe("AnalyticsDashboard", () => {
  it("renders KPI cards from resolved data (A1 bug fix)", async () => {
    render(<AnalyticsDashboard />);
    await waitFor(() => {
      expect(screen.getByText("SLA Compliance")).toBeInTheDocument();
      expect(screen.getByText("97.6%")).toBeInTheDocument();
      expect(screen.getByText("Avg Latency")).toBeInTheDocument();
    });
  });
});
