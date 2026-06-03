import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeAll } from "vitest";

beforeAll(() => {
  window.HTMLElement.prototype.scrollIntoView = vi.fn();
});

// Mock PdfJsViewer + PdfViewer to keep test light (avoid pdfjs-dist load)
vi.mock("../components/chat/PdfJsViewer", () => ({
  default: ({ onClose }) => (
    <div>
      <span>PdfJsViewer Stub</span>
      <button onClick={onClose}>Close-PDF</button>
    </div>
  ),
}));
vi.mock("../components/chat/PdfViewer", () => ({
  default: () => <div>PdfViewer Stub</div>,
}));
vi.mock("../api/documents", () => ({
  getDocument: vi.fn().mockResolvedValue({ data: null }),
}));

import SourceDocPanel from "../components/chat/SourceDocPanel";

describe("SourceDocPanel", () => {
  it("shows empty state when no chunks", () => {
    render(<SourceDocPanel chunks={[]} />);
    expect(screen.getByText(/source citations will appear/i)).toBeInTheDocument();
  });

  it("groups chunks by document and shows source switcher", async () => {
    const chunks = [
      { documentId: 1, documentTitle: "alpha.pdf", pageNumber: 1, chunkText: "A" },
      { documentId: 2, documentTitle: "beta.pdf", pageNumber: 3, chunkText: "B" },
    ];
    render(<SourceDocPanel chunks={chunks} />);
    await waitFor(() => {
      expect(screen.getByText("alpha.pdf")).toBeInTheDocument();
      expect(screen.getByText("beta.pdf")).toBeInTheDocument();
    });
  });

  it("hides PDF viewer when close clicked, shows restore button", async () => {
    const chunks = [
      { documentId: 1, documentTitle: "alpha.pdf", pageNumber: 1, chunkText: "A" },
    ];
    render(<SourceDocPanel chunks={chunks} />);

    await waitFor(() => expect(screen.getByText("PdfJsViewer Stub")).toBeInTheDocument());
    fireEvent.click(screen.getByText("Close-PDF"));
    await waitFor(() => {
      expect(screen.queryByText("PdfJsViewer Stub")).not.toBeInTheDocument();
      expect(screen.getByText("Show PDF preview")).toBeInTheDocument();
    });
  });
});
