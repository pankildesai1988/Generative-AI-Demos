import { describe, it, expect } from "vitest";
import { exportChatToPdf } from "../utils/exportChat";

describe("exportChatToPdf", () => {
  it("produces a non-empty PDF blob from messages", () => {
    const pdf = exportChatToPdf({
      messages: [
        { role: "user", text: "What is RAG?" },
        { role: "assistant", text: "Retrieval-Augmented Generation." },
      ],
      selectedDocuments: ["doc1.pdf"],
    });
    const blob = pdf.output("blob");
    expect(blob.size).toBeGreaterThan(500);
  });

  it("handles empty message list", () => {
    const pdf = exportChatToPdf({ messages: [], selectedDocuments: [] });
    const blob = pdf.output("blob");
    expect(blob.size).toBeGreaterThan(0);
  });
});
