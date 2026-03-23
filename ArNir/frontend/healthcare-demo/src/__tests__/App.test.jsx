import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

// Mock the shared module
vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  ErrorBoundary: ({ children }) => <div>{children}</div>,
  AnalyticsProvider: ({ children }) => <div>{children}</div>,
  useTheme: () => ({
    name: "Healthcare Knowledge Assistant",
    chartPrimary: "#14b8a6",
  }),
  useChatStream: () => ({
    messages: [],
    sendMessage: vi.fn(),
    loading: false,
    lastHistoryId: null,
    chunks: [],
    error: null,
    clearChat: vi.fn(),
  }),
  ChatWindow: ({ title }) => <div data-testid="chat-window">{title}</div>,
  SourceViewer: () => <div data-testid="source-viewer" />,
  useFileUpload: () => ({
    uploadFile: vi.fn(),
    uploading: false,
    error: null,
    result: null,
    reset: vi.fn(),
  }),
  FileUpload: () => <div data-testid="file-upload" />,
}));

import App from "../App";

describe("Healthcare Demo App", () => {
  it("renders chat page on root route", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Medical Knowledge Assistant" })).toBeInTheDocument();
  });

  it("renders upload page on /upload route", () => {
    window.history.pushState({}, "", "/upload");
    render(<App />);
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Medical Documents")).toBeInTheDocument();
  });

  it("renders sidebar navigation", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Ask a Question")).toBeInTheDocument();
    expect(screen.getByText("Upload Documents")).toBeInTheDocument();
  });

  it("renders healthcare branding", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Healthcare")).toBeInTheDocument();
    expect(screen.getByText("Knowledge Assistant")).toBeInTheDocument();
  });
});
