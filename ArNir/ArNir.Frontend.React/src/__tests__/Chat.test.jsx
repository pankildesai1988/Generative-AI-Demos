import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeAll } from "vitest";
import { MemoryRouter } from "react-router-dom";
import Chat from "../components/chat/Chat";

beforeAll(() => {
  window.HTMLElement.prototype.scrollIntoView = vi.fn();
});

vi.mock("../hooks/useChatStream", () => ({
  useChatStream: () => ({
    messages: [
      { role: "user", text: "Hello" },
      { role: "assistant", text: "Hi there!", chunks: [], streaming: false, historyId: 42 },
    ],
    loading: false,
    lastHistoryId: 42,
    lastChunks: [],
    lastConfidence: "medium",
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

vi.mock("../api/documents", () => ({
  listDocuments: vi.fn().mockResolvedValue({ data: [] }),
  getDocument: vi.fn().mockResolvedValue({ data: null }),
}));

function renderChat() {
  return render(
    <MemoryRouter>
      <Chat />
    </MemoryRouter>
  );
}

describe("Chat component", () => {
  it("renders user and assistant messages", () => {
    renderChat();
    expect(screen.getByText("Hello")).toBeInTheDocument();
    expect(screen.getByText(/hi there/i)).toBeInTheDocument();
  });

  it("renders send button", () => {
    renderChat();
    expect(screen.getByRole("button", { name: /send/i })).toBeInTheDocument();
  });

  it("renders rate response link when historyId present", () => {
    renderChat();
    expect(screen.getByText(/rate response/i)).toBeInTheDocument();
  });

  it("renders 3-panel layout (Document Scope + Source Documents)", () => {
    renderChat();
    expect(screen.getByText("Document Scope")).toBeInTheDocument();
    expect(screen.getByText("Source Documents")).toBeInTheDocument();
  });

  it("renders confidence badge", () => {
    renderChat();
    expect(screen.getByText(/medium confidence/i)).toBeInTheDocument();
  });
});
