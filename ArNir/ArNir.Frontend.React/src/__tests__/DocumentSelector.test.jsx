import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import DocumentSelector from "../components/chat/DocumentSelector";

const docs = [
  { id: 1, name: "alpha.pdf", type: "application/pdf", chunks: [1, 2] },
  { id: 2, name: "beta.txt", type: "text/plain", chunks: [1] },
];

describe("DocumentSelector", () => {
  it("renders documents", () => {
    render(<DocumentSelector documents={docs} selectedIds={[]} onToggle={vi.fn()} />);
    expect(screen.getByText("alpha.pdf")).toBeInTheDocument();
    expect(screen.getByText("beta.txt")).toBeInTheDocument();
  });

  it("fires onToggle when checkbox clicked", () => {
    const onToggle = vi.fn();
    render(<DocumentSelector documents={docs} selectedIds={[]} onToggle={onToggle} />);
    fireEvent.click(screen.getAllByRole("checkbox")[0]);
    expect(onToggle).toHaveBeenCalledWith(1);
  });

  it("fires onSelectAll and onClear", () => {
    const onSelectAll = vi.fn();
    const onClear = vi.fn();
    render(
      <DocumentSelector
        documents={docs}
        selectedIds={[]}
        onToggle={vi.fn()}
        onSelectAll={onSelectAll}
        onClear={onClear}
      />
    );
    fireEvent.click(screen.getByText("Select all"));
    fireEvent.click(screen.getByText("Clear selection"));
    expect(onSelectAll).toHaveBeenCalled();
    expect(onClear).toHaveBeenCalled();
  });

  it("shows empty state when no documents", () => {
    render(<DocumentSelector documents={[]} selectedIds={[]} onToggle={vi.fn()} />);
    expect(screen.getByText(/no ingested documents/i)).toBeInTheDocument();
  });
});
