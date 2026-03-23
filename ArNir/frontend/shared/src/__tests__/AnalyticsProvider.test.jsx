import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AnalyticsProvider } from "../analytics/AnalyticsProvider";

const trackEventMock = vi.fn();

vi.mock("../analytics/tracker", async () => {
  const actual = await vi.importActual("../analytics/tracker");
  return {
    ...actual,
    trackEvent: (...args) => trackEventMock(...args),
  };
});

describe("AnalyticsProvider", () => {
  beforeEach(() => {
    trackEventMock.mockReset();
  });

  it("tracks a page view for the current route", async () => {
    render(
      <MemoryRouter initialEntries={["/finance?mode=compare"]}>
        <AnalyticsProvider>
          <Routes>
            <Route path="/finance" element={<div>Finance page</div>} />
          </Routes>
        </AnalyticsProvider>
      </MemoryRouter>
    );

    expect(screen.getByText("Finance page")).toBeInTheDocument();

    await waitFor(() =>
      expect(trackEventMock).toHaveBeenCalledWith(
        "navigation",
        "page_view",
        "/finance",
        { search: "?mode=compare" }
      )
    );
  });
});
