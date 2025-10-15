import jsPDF from "jspdf";
import html2canvas from "html2canvas";

export default function ReportPreview({ report }) {
  if (!report) return null;

  const exportToPDF = async () => {
    const element = document.getElementById("report-preview");
    if (!element) return;
    const canvas = await html2canvas(element, { scale: 2 });
    const pdf = new jsPDF("p", "mm", "a4");
    const imgData = canvas.toDataURL("image/png");
    const imgWidth = 190;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    pdf.addImage(imgData, "PNG", 10, 10, imgWidth, imgHeight);
    pdf.save("AI_Insight_Report.pdf");
  };

  return (
    <div id="report-preview" className="mt-6 p-4 bg-gray-50 border rounded shadow-sm">
      <h4 className="font-semibold text-gray-700 mb-2">Generated Report</h4>
      <pre className="whitespace-pre-wrap text-gray-800">{report}</pre>
      <button
        onClick={exportToPDF}
        className="mt-3 bg-gray-700 text-white px-3 py-1 rounded hover:bg-gray-800"
      >
        Export PDF
      </button>
    </div>
  );
}
