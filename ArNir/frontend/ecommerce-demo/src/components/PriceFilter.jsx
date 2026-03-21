import { DollarSign } from "lucide-react";

export default function PriceFilter({ minPrice, maxPrice, onMinChange, onMaxChange, onReset }) {
  return (
    <div className="rounded-2xl border border-orange-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <DollarSign className="text-primary-600 dark:text-primary-400" size={18} />
          <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100">
            Budget Range
          </h3>
        </div>
        <button
          type="button"
          onClick={onReset}
          className="text-xs font-medium text-primary-600 dark:text-primary-400"
        >
          Reset
        </button>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3">
        <label className="text-xs text-gray-500 dark:text-gray-400">
          Min
          <input
            type="number"
            min="0"
            value={minPrice}
            onChange={(event) => onMinChange(event.target.value)}
            className="mt-1 w-full rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-primary-400 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-100"
            placeholder="0"
          />
        </label>
        <label className="text-xs text-gray-500 dark:text-gray-400">
          Max
          <input
            type="number"
            min="0"
            value={maxPrice}
            onChange={(event) => onMaxChange(event.target.value)}
            className="mt-1 w-full rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-primary-400 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-100"
            placeholder="2500"
          />
        </label>
      </div>
    </div>
  );
}
