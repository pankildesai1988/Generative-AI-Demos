import { jsPDF } from "jspdf";

export function exportChatToPdf({ messages = [], selectedDocuments = [] }) {
  const pdf = new jsPDF();
  const margin = 16;
  const pageHeight = pdf.internal.pageSize.getHeight();
  let y = 20;

  const writeLine = (text, options = {}) => {
    const { size = 11, style = "normal", color = [33, 37, 41] } = options;
    pdf.setFont("helvetica", style);
    pdf.setFontSize(size);
    pdf.setTextColor(...color);

    const lines = pdf.splitTextToSize(text, 178);
    lines.forEach((line) => {
      if (y > pageHeight - 16) {
        pdf.addPage();
        y = 20;
      }
      pdf.text(line, margin, y);
      y += size * 0.45 + 3;
    });
  };

  writeLine("ArNir Healthcare Chat Export", { size: 16, style: "bold", color: [15, 118, 110] });
  writeLine(
    selectedDocuments.length > 0
      ? `Document filter: ${selectedDocuments.join(", ")}`
      : "Document filter: All available documents",
    { size: 10, color: [90, 98, 104] }
  );
  y += 4;

  messages.forEach((message) => {
    const speaker = message.role === "user" ? "Clinician" : "Assistant";
    writeLine(`${speaker}:`, { size: 11, style: "bold" });
    writeLine(message.text, {
      size: 11,
      color: message.isError ? [185, 28, 28] : [33, 37, 41],
    });
    y += 3;
  });

  return pdf;
}
