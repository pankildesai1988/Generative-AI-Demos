export default function FiltersBar({ filters, onChange }) {
  return (
    <div className="flex flex-wrap gap-4 bg-white p-4 rounded-2xl shadow-md">
      <select
        name="provider"
        value={filters.provider}
        onChange={onChange}
        className="border p-2 rounded-xl"
      >
        <option value="">All Providers</option>
        <option value="OpenAI">OpenAI</option>
        <option value="Gemini">Gemini</option>
        <option value="Claude">Claude</option>
      </select>

      <select
        name="model"
        value={filters.model}
        onChange={onChange}
        className="border p-2 rounded-xl"
      >
        <option value="">All Models</option>
        <option value="gpt-4o-mini">gpt-4o-mini</option>
        <option value="gemini-1.5-pro">gemini-1.5-pro</option>
      </select>
    </div>
  );
}
