import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  ErrorBoundary: ({ children }) => <div>{children}</div>,
  useTheme: () => ({
    name: "Financial Document Analyzer",
    chartPrimary: "#1e3a5f",
    mode: "light",
    toggleMode: vi.fn(),
  }),
  useChat: () => ({
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

describe("Finance Demo App", () => {
  it("renders analyzer page on root route", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Financial Document Analyzer" })).toBeInTheDocument();
  });

  it("renders upload page on /upload route", () => {
    window.history.pushState({}, "", "/upload");
    render(<App />);
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Financial Reports")).toBeInTheDocument();
  });

  it("renders finance branding with dark sidebar", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Financial")).toBeInTheDocument();
    expect(screen.getByText("Document Analyzer")).toBeInTheDocument();
  });

  it("renders sidebar navigation", () => {
    window.history.pushState({}, "", "/");
    render(<App />);
    expect(screen.getByText("Analyze Documents")).toBeInTheDocument();
    expect(screen.getByText("Upload Reports")).toBeInTheDocument();
    expect(screen.getByText("Compare Analyses")).toBeInTheDocument();
  });
});
