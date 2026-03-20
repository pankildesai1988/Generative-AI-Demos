import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("@arnir/shared", () => ({
  useFileUpload: () => ({
    uploadFile: vi.fn(),
    uploading: false,
    error: null,
    result: null,
    reset: vi.fn(),
  }),
  FileUpload: ({ guidance }) => (
    <div data-testid="file-upload">{guidance}</div>
  ),
}));

vi.mock("react-hot-toast", () => ({
  default: { success: vi.fn(), error: vi.fn() },
}));

import MedicalUploadPage from "../components/MedicalUploadPage";

describe("MedicalUploadPage", () => {
  it("renders upload section with medical guidance", () => {
    render(<MedicalUploadPage />);
    expect(screen.getByText("Upload Medical Documents")).toBeInTheDocument();
    expect(
      screen.getByText(
        "Drag and drop medical documents here, or click to browse"
      )
    ).toBeInTheDocument();
  });

  it("renders sample data section", () => {
    render(<MedicalUploadPage />);
    expect(screen.getByText("Try Sample Data")).toBeInTheDocument();
  });

  it("renders all 3 sample files", () => {
    render(<MedicalUploadPage />);
    expect(
      screen.getByText("WHO Hypertension Management Guidelines 2024")
    ).toBeInTheDocument();
    expect(
      screen.getByText("Type 2 Diabetes Treatment Protocol")
    ).toBeInTheDocument();
    expect(
      screen.getByText("Common Drug Interaction Reference")
    ).toBeInTheDocument();
  });

  it("renders upload buttons for each sample", () => {
    render(<MedicalUploadPage />);
    const uploadButtons = screen.getAllByText("Upload Sample");
    expect(uploadButtons).toHaveLength(3);
  });
});
