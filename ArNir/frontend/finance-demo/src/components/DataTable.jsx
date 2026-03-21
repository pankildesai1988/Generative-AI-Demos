import { useMemo, useState } from "react";
import { ArrowUpDown } from "lucide-react";

function sortRows(rows, index, direction) {
  return [...rows].sort((left, right) => {
    const a = left[index] || "";
    const b = right[index] || "";
    const maybeNumA = Number(String(a).replace(/[$,%(),]/g, ""));
    const maybeNumB = Number(String(b).replace(/[$,%(),]/g, ""));

    const bothNumbers = !Number.isNaN(maybeNumA) && !Number.isNaN(maybeNumB);
    const comparison = bothNumbers
      ? maybeNumA - maybeNumB
      : String(a).localeCompare(String(b));

    return direction === "asc" ? comparison : -comparison;
  });
}

export default function DataTable({ table }) {
  const [sortState, setSortState] = useState({ index: 0, direction: "asc" });

  const sortedRows = useMemo(
    () => sortRows(table.rows, sortState.index, sortState.direction),
    [table.rows, sortState]
  );

  const handleSort = (index) => {
    setSortState((current) => ({
      index,
      direction:
        current.index === index && current.direction === "asc" ? "desc" : "asc",
    }));
  };

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-900">
      <h4 className="mb-3 text-sm font-semibold text-gray-900 dark:text-gray-100">
        Extracted Data Table
      </h4>
      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200 dark:border-gray-700">
              {table.headers.map((header, index) => (
                <th key={header} className="px-3 py-2 text-left text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">
                  <button
                    type="button"
                    onClick={() => handleSort(index)}
                    className="inline-flex items-center gap-2"
                  >
                    {header}
                    <ArrowUpDown size={12} />
                  </button>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {sortedRows.map((row, rowIndex) => (
              <tr key={`${rowIndex}-${row.join("-")}`} className="border-b border-gray-100 dark:border-gray-800">
                {row.map((cell, cellIndex) => (
                  <td key={`${rowIndex}-${cellIndex}`} className="px-3 py-2 text-gray-800 dark:text-gray-200">
                    {cell}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
