import { render, screen, fireEvent } from "@testing-library/react";
import ErrorBanner from "../components/ErrorBanner";

describe("ErrorBanner", () => {
  it("returns null when no message", () => {
    const { container } = render(<ErrorBanner message={null} />);
    expect(container.firstChild).toBeNull();
  });

  it("renders error message", () => {
    render(<ErrorBanner message="Something failed" />);
    expect(screen.getByText("Something failed")).toBeInTheDocument();
  });

  it("renders retry button when onRetry provided", () => {
    const onRetry = vi.fn();
    render(<ErrorBanner message="Error" onRetry={onRetry} />);
    fireEvent.click(screen.getByText("Retry"));
    expect(onRetry).toHaveBeenCalled();
  });
});
