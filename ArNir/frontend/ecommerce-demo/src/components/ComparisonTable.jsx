import { Scale } from "lucide-react";
import { formatCurrency } from "../utils/productData";

const comparisonRows = [
  { key: "priceValue", label: "Price", format: (value) => formatCurrency(value) },
  { key: "category", label: "Category" },
  { key: "cpu", label: "CPU" },
  { key: "ram", label: "RAM" },
  { key: "storage", label: "Storage" },
  { key: "display", label: "Display" },
  { key: "gpu", label: "GPU" },
  { key: "battery", label: "Battery" },
  { key: "weight", label: "Weight" },
  { key: "bestFor", label: "Best For" },
];

export default function ComparisonTable({ products = [], onClear }) {
  if (products.length !== 2) {
    return null;
  }

  return (
    <div className="rounded-2xl border border-primary-200 bg-white p-4 shadow-sm dark:border-primary-800 dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Scale className="text-primary-600 dark:text-primary-400" size={18} />
          <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100">
            Side-by-Side Comparison
          </h3>
        </div>
        <button
          type="button"
          onClick={onClear}
          className="text-xs font-medium text-primary-600 dark:text-primary-400"
        >
          Clear
        </button>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200 dark:border-gray-700">
              <th className="px-3 py-2 text-left text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">
                Attribute
              </th>
              {products.map((product) => (
                <th key={product.id} className="px-3 py-2 text-left font-semibold text-gray-900 dark:text-gray-100">
                  {product.title}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {comparisonRows.map((row) => (
              <tr key={row.key} className="border-b border-gray-100 dark:border-gray-800">
                <td className="px-3 py-2 text-gray-500 dark:text-gray-400">{row.label}</td>
                {products.map((product) => (
                  <td key={`${product.id}-${row.key}`} className="px-3 py-2 text-gray-800 dark:text-gray-200">
                    {row.format ? row.format(product[row.key]) : product[row.key] || "N/A"}
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
