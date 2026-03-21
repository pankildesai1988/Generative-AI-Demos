import {
  ResponsiveContainer,
  BarChart,
  Bar,
  CartesianGrid,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

export default function FinanceChart({ data = [] }) {
  if (data.length === 0) {
    return null;
  }

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-900">
      <h4 className="mb-3 text-sm font-semibold text-gray-900 dark:text-gray-100">
        Extracted Financial Metrics
      </h4>
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data} margin={{ top: 8, right: 12, left: 0, bottom: 24 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#33415522" />
            <XAxis dataKey="label" tick={{ fontSize: 11 }} angle={-18} textAnchor="end" height={42} />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(value, name, item) => [item.payload.formatted || value, "Value"]}
              contentStyle={{ borderRadius: 12, borderColor: "#cbd5e1" }}
            />
            <Bar dataKey="value" fill="#f59e0b" radius={[8, 8, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
