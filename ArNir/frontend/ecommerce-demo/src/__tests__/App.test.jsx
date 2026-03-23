import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  ErrorBoundary: ({ children }) => <div>{children}</div>,
  AnalyticsProvider: ({ children }) => <div>{children}</div>,
  useTheme: () => ({
    name: "Ecommerce Product Advisor",
    chartPrimary: "#f97316",
    mode: "light",
    toggleMode: vi.fn(),
  }),
  useChatStream: () => ({
    messages: [], sendMessage: vi.fn(), loading: false,
    lastHistoryId: null, chunks: [], error: null, clearChat: vi.fn(),
  }),
  ChatWindow: ({ title }) => <div data-testid="chat-window">{title}</div>,
  SourceViewer: () => <div data-testid="source-viewer" />,
  useFileUpload: () => ({
    uploadFile: vi.fn(), uploading: false, error: null, result: null, reset: vi.fn(),
  }),
  FileUpload: () => <div data-testid="file-upload" />,
}));

import App from "../App";

describe("Ecommerce Demo App", () => {
  it("renders product advisor page on root route", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getAllByText("Product Advisor").length).toBeGreaterThan(0);
  });

  it("renders upload page on /upload route", () => {
    window.history.pushState({}, "", "/upload");
    render(<App />);
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Product Catalog")).toBeInTheDocument();
  });

  it("renders ecommerce branding", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Ecommerce")).toBeInTheDocument();
    expect(screen.getAllByText("Product Advisor").length).toBeGreaterThan(0);
  });

  it("renders sidebar navigation", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Upload Catalog")).toBeInTheDocument();
  });
});
