import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect, vi, beforeAll } from "vitest";
import { ThemeProvider } from "../context/ThemeContext";
import ErrorBoundary from "../components/shared/ErrorBoundary";
import DocumentUploadPage from "../pages/DocumentUploadPage";
import Chat from "../components/chat/Chat";

beforeAll(() => {
  window.HTMLElement.prototype.scrollIntoView = vi.fn();
});

vi.mock("../api/client", () => ({ default: { get: vi.fn().mockResolvedValue({ data: [] }), post: vi.fn() } }));
vi.mock("../api/documents", () => ({
  listDocuments: vi.fn().mockResolvedValue({ data: [] }),
  uploadDocument: vi.fn(),
  getDocument: vi.fn().mockResolvedValue({ data: null }),
}));
vi.mock("../hooks/useChatStream", () => ({
  useChatStream: () => ({
    messages: [],
    loading: false,
    lastHistoryId: null,
    lastChunks: [],
    lastConfidence: "low",
    sendMessage: vi.fn(),
    clearMessages: vi.fn(),
  }),
}));
vi.mock("../hooks/useDocumentList", () => ({
  default: () => ({
    documents: [],
    loading: false,
    error: null,
    refreshDocuments: vi.fn(),
  }),
}));

function wrap(ui) {
  return render(
    <MemoryRouter>
      <ThemeProvider>
        <ErrorBoundary>{ui}</ErrorBoundary>
      </ThemeProvider>
    </MemoryRouter>
  );
}

describe("App routing", () => {
  it("renders chat page", () => {
    wrap(<Chat />);
    expect(screen.getByPlaceholderText(/ask about/i)).toBeInTheDocument();
  });

  it("renders upload page", () => {
    wrap(<DocumentUploadPage />);
    expect(screen.getByText(/upload documents/i)).toBeInTheDocument();
  });
});
