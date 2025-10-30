import React from "react";

export default function AlertList({ alerts = [] }) {
  if (!alerts.length)
    return <div className="text-gray-400 text-sm">✅ No active alerts for selected filters.</div>;

  return (
    <div className="rounded-xl border bg-white p-4 shadow-sm">
      <h3 className="font-semibold mb-2 text-gray-800">Active Alerts</h3>
      <ul className="list-disc pl-5 text-sm text-gray-700 space-y-1 max-h-64 overflow-y-auto">
        {alerts.map((a, i) => (
          <li key={i}>
            <strong className="capitalize">{a.type}</strong> — {a.message}
          </li>
        ))}
      </ul>
    </div>
  );
}
