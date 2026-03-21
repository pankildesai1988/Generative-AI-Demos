import { useMemo, useState } from "react";
import ComparisonDashboard from "./ComparisonDashboard";
import useComparisonHistory from "../hooks/useComparisonHistory";

export default function FinanceComparePage() {
  const { entries, clearEntries } = useComparisonHistory();
  const [leftId, setLeftId] = useState(entries[0]?.id || "");
  const [rightId, setRightId] = useState(entries[1]?.id || "");

  const leftEntry = useMemo(
    () => entries.find((entry) => entry.id === leftId),
    [entries, leftId]
  );
  const rightEntry = useMemo(
    () => entries.find((entry) => entry.id === rightId),
    [entries, rightId]
  );

  return (
    <div className="h-full overflow-y-auto p-4 md:p-6">
      <div className="mb-5 flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Compare Analyses</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Review two recent finance answers side by side.
          </p>
        </div>
        <button
          type="button"
          onClick={clearEntries}
          className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-200"
        >
          Clear History
        </button>
      </div>

      <div className="mb-5 grid gap-3 md:grid-cols-2">
        <select
          value={leftId}
          onChange={(event) => setLeftId(event.target.value)}
          className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-900 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100"
        >
          <option value="">Select first analysis</option>
          {entries.map((entry) => (
            <option key={entry.id} value={entry.id}>
              {entry.query}
            </option>
          ))}
        </select>
        <select
          value={rightId}
          onChange={(event) => setRightId(event.target.value)}
          className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-900 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100"
        >
          <option value="">Select second analysis</option>
          {entries.map((entry) => (
            <option key={entry.id} value={entry.id}>
              {entry.query}
            </option>
          ))}
        </select>
      </div>

      <ComparisonDashboard leftEntry={leftEntry} rightEntry={rightEntry} />
    </div>
  );
}
