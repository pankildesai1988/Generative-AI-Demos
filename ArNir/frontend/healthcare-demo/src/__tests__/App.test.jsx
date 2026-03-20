import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";

// Mock the shared module
vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  useTheme: () => ({
    name: "Healthcare Knowledge Assistant",
    chartPrimary: "#14b8a6",
  }),
  useChat: () => ({
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
    render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>
    );
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getByText("Medical Knowledge Assistant")).toBeInTheDocument();
  });

  it("renders upload page on /upload route", () => {
    render(
      <MemoryRouter initialEntries={["/upload"]}>
        <App />
      </MemoryRouter>
    );
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Medical Documents")).toBeInTheDocument();
  });

  it("renders sidebar navigation", () => {
    render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>
    );
    expect(screen.getByText("Ask a Question")).toBeInTheDocument();
    expect(screen.getByText("Upload Documents")).toBeInTheDocument();
  });

  it("renders healthcare branding", () => {
    render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>
    );
    expect(screen.getByText("Healthcare")).toBeInTheDocument();
    expect(screen.getByText("Knowledge Assistant")).toBeInTheDocument();
  });
});
