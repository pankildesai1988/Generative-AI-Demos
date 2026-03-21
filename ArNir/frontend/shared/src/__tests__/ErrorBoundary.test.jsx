import { render, screen, fireEvent } from "@testing-library/react";
import ErrorBoundary from "../components/ErrorBoundary";

const ThrowError = () => { throw new Error("Test crash"); };

describe("ErrorBoundary", () => {
  // suppress error boundary logs in test
  const spy = vi.spyOn(console, "error").mockImplementation(() => {});
  afterAll(() => spy.mockRestore());

  it("renders children when no error", () => {
    render(<ErrorBoundary><p>Hello</p></ErrorBoundary>);
    expect(screen.getByText("Hello")).toBeInTheDocument();
  });

  it("shows fallback on error", () => {
    render(<ErrorBoundary><ThrowError /></ErrorBoundary>);
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
    expect(screen.getByText("Test crash")).toBeInTheDocument();
  });

  it("retry resets error state", () => {
    let shouldThrow = true;
    const MaybeThrow = () => { if (shouldThrow) throw new Error("oops"); return <p>Recovered</p>; };
    render(<ErrorBoundary><MaybeThrow /></ErrorBoundary>);
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
    shouldThrow = false;
    fireEvent.click(screen.getByText("Try Again"));
    expect(screen.getByText("Recovered")).toBeInTheDocument();
  });
});
