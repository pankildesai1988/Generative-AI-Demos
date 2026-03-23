import { render, screen, fireEvent } from "@testing-library/react";
import SourceViewer from "../components/SourceViewer";

describe("SourceViewer", () => {
  it("returns null with no chunks", () => {
    const { container } = render(<SourceViewer chunks={[]} />);
    expect(container.firstChild).toBeNull();
  });

  it("renders chunk list", () => {
    const chunks = [
      { documentTitle: "Doc A", chunkText: "Content A", rank: 1 },
      { documentTitle: "Doc B", chunkText: "Content B", rank: 2 },
    ];
    render(<SourceViewer chunks={chunks} />);
    expect(screen.getByText("Sources (2)")).toBeInTheDocument();
    expect(screen.getByText("— Doc A")).toBeInTheDocument();
  });

  it("expands chunk on click", () => {
    const chunks = [{ documentTitle: "Doc", chunkText: "Detailed content here" }];
    render(<SourceViewer chunks={chunks} />);
    fireEvent.click(screen.getByText("Source 1"));
    expect(screen.getByText("Detailed content here")).toBeInTheDocument();
  });
});
