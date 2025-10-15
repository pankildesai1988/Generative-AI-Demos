export default function FiltersBar({ filters, setFilters }) {
  const handleChange = (key, value) => setFilters((prev) => ({ ...prev, [key]: value }));

  return (
    <div className="flex flex-wrap gap-3 bg-white border p-3 rounded shadow-sm items-center">
      <div>
        <label className="text-sm font-medium text-gray-600 mr-2">Provider:</label>
        <select
          className="border rounded p-1"
          value={filters.provider}
          onChange={(e) => handleChange("provider", e.target.value)}
        >
          <option value="All">All</option>
          <option value="OpenAI">OpenAI</option>
          <option value="Claude">Claude</option>
          <option value="Gemini">Gemini</option>
        </select>
      </div>

      <div>
        <label className="text-sm font-medium text-gray-600 mr-2">Date Range:</label>
        <select
          className="border rounded p-1"
          value={filters.dateRange}
          onChange={(e) => handleChange("dateRange", e.target.value)}
        >
          <option value="1d">Last 24h</option>
          <option value="7d">Last 7 Days</option>
          <option value="30d">Last 30 Days</option>
        </select>
      </div>
    </div>
  );
}
