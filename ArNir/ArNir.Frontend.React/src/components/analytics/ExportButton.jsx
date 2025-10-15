import jsPDF from "jspdf";
import html2canvas from "html2canvas";

export default function ExportButton({ data }) {
  const handleExport = async () => {
    const element = document.getElementById("analytics-dashboard");
    if (!element) return;
    const canvas = await html2canvas(element, { scale: 2 });
    const pdf = new jsPDF("p", "mm", "a4");
    const imgData = canvas.toDataURL("image/png");
    const imgWidth = 190;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    pdf.addImage(imgData, "PNG", 10, 10, imgWidth, imgHeight);
    pdf.save("Analytics_Report.pdf");
  };

  return (
    <button
      onClick={handleExport}
      className="bg-gray-800 text-white px-4 py-2 rounded hover:bg-gray-900"
    >
      Export PDF
    </button>
  );
}
