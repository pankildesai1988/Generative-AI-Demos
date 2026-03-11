import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";

export default function InsightChartCard({ response }) {
  if (!response.chart) return null;
  const data = response.chart.data;

  return (
    <div className="bg-white shadow rounded-lg p-4 mt-4">
      <h3 className="font-bold mb-2">{response.chart.title}</h3>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={data}>
          <XAxis dataKey="label" />
          <YAxis />
          <Tooltip />
          <Bar dataKey="value" fill="#2563eb" />
        </BarChart>
      </ResponsiveContainer>
      <p className="mt-4 text-gray-700">{response.responseText}</p>
    </div>
  );
}
