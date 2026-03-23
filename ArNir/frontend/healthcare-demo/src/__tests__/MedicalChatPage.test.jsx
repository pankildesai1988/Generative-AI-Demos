import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("../hooks/useDocumentList", () => ({
  default: () => ({
    documents: [{ id: 1, name: "Cardiology Guidelines", type: "pdf", chunks: [{}] }],
    loading: false,
    error: null,
    refreshDocuments: vi.fn(),
  }),
}));

vi.mock("../components/ExportButton", () => ({
  default: () => <div data-testid="export-button">Export Chat</div>,
}));

vi.mock("../components/SourceDocPanel", () => ({
  default: ({ chunks }) => (
    <div data-testid="source-doc-panel">
      Source Documents
      <span>{chunks.length} retrieved chunk</span>
    </div>
  ),
}));

vi.mock("@arnir/shared", () => ({
  useChatStream: () => ({
    messages: [],
    sendMessage: vi.fn(),
    loading: false,
    lastHistoryId: null,
    chunks: [
      {
        chunkText: "Sample medical text",
        documentId: 1,
        documentTitle: "Guidelines",
        rank: 1,
      },
    ],
    error: null,
    clearChat: vi.fn(),
  }),
  ChatWindow: ({ title, placeholder }) => (
    <div data-testid="chat-window">
      <span>{title}</span>
      <span>{placeholder}</span>
    </div>
  ),
  MessageBubble: ({ text }) => <div>{text}</div>,
}));

import MedicalChatPage from "../components/MedicalChatPage";

describe("MedicalChatPage", () => {
  it("renders chat window with medical title", () => {
    render(<MedicalChatPage />);
    expect(screen.getByRole("heading", { name: "Medical Knowledge Assistant" })).toBeInTheDocument();
  });

  it("renders source citation panel", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText("Source Documents")).toBeInTheDocument();
  });

  it("passes medical placeholder to chat", () => {
    render(<MedicalChatPage />);
    expect(
      screen.getByText("Ask about symptoms, treatments, drug interactions...")
    ).toBeInTheDocument();
  });

  it("shows source count when chunks available", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText(/1 retrieved chunk/)).toBeInTheDocument();
  });

  it("renders the document selector and export action", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText("Document Scope")).toBeInTheDocument();
    expect(screen.getByTestId("export-button")).toBeInTheDocument();
  });
});
