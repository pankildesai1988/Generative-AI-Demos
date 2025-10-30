import React from "react";
import { Button } from "../ui/button";

/**
 * Export control panel: CSV / Excel / PDF
 */
export default function ExportPanel({ onExport }) {
  return (
    <div className="flex flex-wrap gap-3 mt-4">
      <Button onClick={() => onExport("csv")}>Export CSV</Button>
      <Button onClick={() => onExport("excel")}>Export Excel</Button>
      <Button onClick={() => onExport("pdf")}>Export PDF</Button>
    </div>
  );
}
