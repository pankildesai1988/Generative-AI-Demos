import { Download } from "lucide-react";
import { exportChatToPdf } from "../utils/exportChat";

export default function ExportButton({ messages = [], selectedDocuments = [] }) {
  const handleExport = () => {
    const pdf = exportChatToPdf({
      messages,
      selectedDocuments,
    });
    pdf.save("arnir-healthcare-chat.pdf");
  };

  return (
    <button
      type="button"
      onClick={handleExport}
      disabled={messages.length === 0}
      className="inline-flex items-center gap-2 rounded-xl bg-primary-600 px-3 py-2 text-sm font-medium text-white hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50 transition"
    >
      <Download size={16} />
      Export Chat
    </button>
  );
}
