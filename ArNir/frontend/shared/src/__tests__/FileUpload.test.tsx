import { render, screen, fireEvent } from "@testing-library/react";
import FileUpload from "../components/FileUpload";

describe("FileUpload", () => {
  const defaultProps = {
    onUpload: vi.fn(),
    uploading: false,
    error: null,
    result: null,
    onReset: vi.fn(),
  };

  it("renders drop zone", () => {
    render(<FileUpload {...defaultProps} />);
    expect(screen.getByText(/Drag and drop/)).toBeInTheDocument();
    expect(screen.getByText(/Supported: PDF, TXT, DOCX/)).toBeInTheDocument();
  });

  it("renders custom guidance", () => {
    render(<FileUpload {...defaultProps} guidance="Upload medical files" />);
    expect(screen.getByText("Upload medical files")).toBeInTheDocument();
  });

  it("shows error message", () => {
    render(<FileUpload {...defaultProps} error="Upload failed" />);
    expect(screen.getByText("Upload failed")).toBeInTheDocument();
  });

  it("shows success result", () => {
    render(<FileUpload {...defaultProps} result={{ message: "Queued", documentId: 42 }} />);
    expect(screen.getByText("Queued")).toBeInTheDocument();
    expect(screen.getByText("Document ID: 42")).toBeInTheDocument();
  });
});
