import { BarChart, Bar, XAxis, YAxis, Tooltip, PieChart, Pie, Cell, Legend, ResponsiveContainer } from "recharts";

export default function AnalyticsCharts({ data }) {
  if (!data || data.length === 0) {
    return <div className="text-gray-500 text-sm mt-5">No analytics data available</div>;
  }

  const COLORS = ["#1d4ed8", "#9333ea", "#f59e0b", "#10b981"];

  const pieData = data.map((d) => ({
    name: d.provider,
    value: d.slaComplianceRate
  }));

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mt-8">
      {/* Bar Chart */}
      <div className="bg-white p-5 rounded-2xl shadow-md">
        <h3 className="font-semibold mb-3 text-gray-700">Average Latency per Model</h3>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={data}>
            <XAxis dataKey="model" />
            <YAxis />
            <Tooltip />
            <Bar dataKey="avgTotalLatencyMs" fill="#1d4ed8" />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Pie Chart */}
      <div className="bg-white p-5 rounded-2xl shadow-md">
        <h3 className="font-semibold mb-3 text-gray-700">SLA Compliance by Provider</h3>
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie data={pieData} dataKey="value" nameKey="name" outerRadius={100}>
              {pieData.map((_, i) => (
                <Cell key={i} fill={COLORS[i % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
