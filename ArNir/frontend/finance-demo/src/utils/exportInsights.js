import { jsPDF } from "jspdf";
import * as XLSX from "xlsx";

export function exportInsightsPdf({ title = "Finance Insights Export", answer = "", chartData = [], risk = null }) {
  const pdf = new jsPDF();
  let y = 18;

  const write = (text, size = 11) => {
    pdf.setFontSize(size);
    const lines = pdf.splitTextToSize(text, 175);
    lines.forEach((line) => {
      if (y > 280) {
        pdf.addPage();
        y = 18;
      }
      pdf.text(line, 16, y);
      y += size * 0.4 + 4;
    });
  };

  write(title, 16);
  if (risk) {
    write(`Risk Score: ${risk.score}/100 (${risk.level})`, 11);
  }
  if (chartData.length > 0) {
    write("Key Metrics:", 12);
    chartData.forEach((item) => write(`${item.label}: ${item.formatted || item.value}`));
  }
  write("Answer Summary:", 12);
  write(answer || "No answer available.");

  pdf.save("arnir-finance-insights.pdf");
}

export function exportInsightsSheet({ chartData = [], risk = null, table = null }) {
  const workbook = XLSX.utils.book_new();

  const metricSheet = XLSX.utils.json_to_sheet(
    chartData.map((item) => ({
      Metric: item.label,
      Value: item.formatted || item.value,
    }))
  );
  XLSX.utils.book_append_sheet(workbook, metricSheet, "Metrics");

  const riskSheet = XLSX.utils.json_to_sheet([
    {
      Score: risk?.score ?? 0,
      Level: risk?.level ?? "Low",
      Factors: risk?.factors?.map((factor) => factor.label).join(", ") || "",
    },
  ]);
  XLSX.utils.book_append_sheet(workbook, riskSheet, "Risk");

  if (table?.headers?.length && table?.rows?.length) {
    const tableRows = table.rows.map((row) =>
      Object.fromEntries(table.headers.map((header, index) => [header, row[index] || ""]))
    );
    const tableSheet = XLSX.utils.json_to_sheet(tableRows);
    XLSX.utils.book_append_sheet(workbook, tableSheet, "Table");
  }

  XLSX.writeFile(workbook, "arnir-finance-insights.xlsx");
}
