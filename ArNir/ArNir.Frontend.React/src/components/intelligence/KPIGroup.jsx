import React from "react";

export default function KPIGroup({ kpis = [] }) {
  if (!kpis.length)
    return <div className="text-gray-400 text-sm">No KPI data available.</div>;

  return (
    <div className="grid sm:grid-cols-3 gap-4">
      {kpis.map((kpi, i) => (
        <div
          key={i}
          className="rounded-xl border bg-white p-4 text-center shadow-sm hover:shadow-md transition"
        >
          <div className="text-gray-500 text-sm">{kpi.label}</div>
          <div className="text-2xl font-semibold text-gray-800">{kpi.value}</div>
          <div
            className={`text-xs ${
              kpi.trend?.startsWith("-") ? "text-red-600" : "text-green-600"
            }`}
          >
            {kpi.trend}
          </div>
        </div>
      ))}
    </div>
  );
}
