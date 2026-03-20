import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";

vi.mock("@arnir/shared", () => ({
  ThemeProvider: ({ children }) => <div>{children}</div>,
  useTheme: () => ({ name: "Ecommerce Product Advisor", chartPrimary: "#f97316" }),
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

describe("Ecommerce Demo App", () => {
  it("renders product advisor page on root route", () => {
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByTestId("chat-window")).toBeInTheDocument();
    expect(screen.getByText("Product Advisor")).toBeInTheDocument();
  });

  it("renders upload page on /upload route", () => {
    render(<MemoryRouter initialEntries={["/upload"]}><App /></MemoryRouter>);
    expect(screen.getByTestId("file-upload")).toBeInTheDocument();
    expect(screen.getByText("Upload Product Catalog")).toBeInTheDocument();
  });

  it("renders ecommerce branding", () => {
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByText("Ecommerce")).toBeInTheDocument();
    expect(screen.getByText("Product Advisor")).toBeInTheDocument();
  });

  it("renders sidebar navigation", () => {
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(screen.getByText("Upload Catalog")).toBeInTheDocument();
  });
});
