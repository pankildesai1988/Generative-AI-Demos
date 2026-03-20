/**
 * Gold-accented metric card for financial data.
 */
export default function MetricsCard({ icon: Icon, label, value, type = "default" }) {
  const borderColor =
    type === "amount"
      ? "border-accent-200"
      : type === "risk"
      ? "border-red-200"
      : "border-gray-200";

  return (
    <div
      className={`flex items-center gap-3 p-3 border ${borderColor} rounded-lg bg-white hover:shadow-sm transition`}
    >
      <div className="w-9 h-9 bg-accent-50 rounded-lg flex items-center justify-center flex-shrink-0">
        <Icon className="text-accent-600" size={18} />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-xs text-gray-500">{label}</p>
        <p className="text-sm font-bold text-gray-900 truncate">{value}</p>
      </div>
    </div>
  );
}
