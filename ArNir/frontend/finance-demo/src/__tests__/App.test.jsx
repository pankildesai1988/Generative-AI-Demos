import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";

vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  useTheme: () => ({ name: "Financial Document Analyzer", chartPrimary: "#1e3a5f" }),
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
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getByText("Financial Document Analyzer")).toBeInTheDocument();
  });

  it("renders upload page on /upload route", () => {
    render(<MemoryRouter initialEntries={["/upload"]}><App /></MemoryRouter>);
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Financial Reports")).toBeInTheDocument();
  });

  it("renders finance branding with dark sidebar", () => {
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByText("Financial")).toBeInTheDocument();
    expect(screen.getByText("Document Analyzer")).toBeInTheDocument();
  });

  it("renders sidebar navigation", () => {
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByText("Analyze Documents")).toBeInTheDocument();
    expect(screen.getByText("Upload Reports")).toBeInTheDocument();
  });
});
