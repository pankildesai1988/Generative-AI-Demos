import { useState } from "react";
import { Download, FileSpreadsheet, FileText } from "lucide-react";
import { exportInsightsPdf, exportInsightsSheet } from "../utils/exportInsights";

export default function ExportMenu({ answer, chartData, risk, table }) {
  const [open, setOpen] = useState(false);

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        className="inline-flex items-center gap-2 rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700 transition hover:border-primary-300 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-200"
      >
        <Download size={16} />
        Export
      </button>

      {open && (
        <div className="absolute right-0 z-20 mt-2 w-48 rounded-xl border border-gray-200 bg-white p-2 shadow-xl dark:border-gray-700 dark:bg-gray-900">
          <button
            type="button"
            onClick={() => {
              exportInsightsPdf({ answer, chartData, risk });
              setOpen(false);
            }}
            className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-800"
          >
            <FileText size={16} />
            Export PDF
          </button>
          <button
            type="button"
            onClick={() => {
              exportInsightsSheet({ chartData, risk, table });
              setOpen(false);
            }}
            className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-800"
          >
            <FileSpreadsheet size={16} />
            Export Excel
          </button>
        </div>
      )}
    </div>
  );
}
