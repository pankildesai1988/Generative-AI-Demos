import React from "react";

/**
 * Provider / date-range filters for the Intelligence Dashboard.
 */
export default function FiltersBar({ filters, onChange }) {
  const handleChange = (key, value) => onChange({ ...filters, [key]: value });

  return (
    <div className="flex flex-wrap gap-3 items-center mb-4">
      {/* Provider */}
      <select
        value={filters.provider}
        onChange={(e) => handleChange("provider", e.target.value)}
        className="border rounded-lg px-3 py-2 text-sm bg-white"
      >
        <option value="">All Providers</option>
        <option value="openai">OpenAI</option>
        <option value="gemini">Gemini</option>
        <option value="claude">Claude</option>
      </select>

      {/* Date Range */}
      <input
        type="date"
        value={filters.startDate}
        onChange={(e) => handleChange("startDate", e.target.value)}
        className="border rounded-lg px-3 py-2 text-sm"
      />
      <input
        type="date"
        value={filters.endDate}
        onChange={(e) => handleChange("endDate", e.target.value)}
        className="border rounded-lg px-3 py-2 text-sm"
      />

      {/* Refresh */}
      <button
        onClick={() => onChange({ ...filters })}
        className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 transition"
      >
        Refresh
      </button>
    </div>
  );
}
