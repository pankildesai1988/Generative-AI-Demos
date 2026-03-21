import { SlidersHorizontal } from "lucide-react";

export default function FacetPanel({
  availableCategories = [],
  availablePriceBands = [],
  selectedCategories = [],
  selectedPriceBands = [],
  onToggleCategory,
  onTogglePriceBand,
  onClear,
}) {
  return (
    <div className="space-y-4 rounded-2xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <SlidersHorizontal className="text-primary-600 dark:text-primary-400" size={18} />
          <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100">
            Facets
          </h3>
        </div>
        <button
          type="button"
          onClick={onClear}
          className="text-xs font-medium text-primary-600 dark:text-primary-400"
        >
          Clear all
        </button>
      </div>

      <div>
        <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
          Categories
        </p>
        <div className="mt-3 space-y-2">
          {availableCategories.map((item) => (
            <label key={item.value} className="flex items-center justify-between gap-3 text-sm text-gray-700 dark:text-gray-200">
              <span className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={selectedCategories.includes(item.value)}
                  onChange={() => onToggleCategory(item.value)}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                {item.value}
              </span>
              <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-500 dark:bg-gray-800 dark:text-gray-400">
                {item.count}
              </span>
            </label>
          ))}
        </div>
      </div>

      <div>
        <p className="text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
          Price Bands
        </p>
        <div className="mt-3 space-y-2">
          {availablePriceBands.map((item) => (
            <label key={item.value} className="flex items-center justify-between gap-3 text-sm text-gray-700 dark:text-gray-200">
              <span className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={selectedPriceBands.includes(item.value)}
                  onChange={() => onTogglePriceBand(item.value)}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                {item.value}
              </span>
              <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-500 dark:bg-gray-800 dark:text-gray-400">
                {item.count}
              </span>
            </label>
          ))}
        </div>
      </div>
    </div>
  );
}
