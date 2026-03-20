import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("@arnir/shared", () => ({
  useChat: () => ({
    messages: [],
    sendMessage: vi.fn(),
    loading: false,
    lastHistoryId: null,
    chunks: [
      { chunkText: "Sample medical text", documentTitle: "Guidelines", rank: 1 },
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
  SourceViewer: ({ chunks, title }) => (
    <div data-testid="source-viewer">
      {title} - {chunks.length} chunks
    </div>
  ),
}));

import MedicalChatPage from "../components/MedicalChatPage";

describe("MedicalChatPage", () => {
  it("renders chat window with medical title", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText("Medical Knowledge Assistant")).toBeInTheDocument();
  });

  it("renders source citation panel", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText("Medical Sources")).toBeInTheDocument();
  });

  it("passes medical placeholder to chat", () => {
    render(<MedicalChatPage />);
    expect(
      screen.getByText("Ask about symptoms, treatments, drug interactions...")
    ).toBeInTheDocument();
  });

  it("shows source count when chunks available", () => {
    render(<MedicalChatPage />);
    expect(screen.getByText(/1 source/)).toBeInTheDocument();
  });
});
