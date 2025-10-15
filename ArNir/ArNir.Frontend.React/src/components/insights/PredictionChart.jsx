import {
  LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer,
} from "recharts";

export default function PredictionChart({ chartData }) {
  if (!chartData || chartData.length === 0) return null;
  return (
    <div className="mt-6">
      <h4 className="font-semibold mb-2 text-gray-700">Trend Visualization</h4>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="index" />
          <YAxis />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey="actual" stroke="#2563eb" name="Historical" strokeWidth={2} />
          <Line type="monotone" dataKey="predicted" stroke="#16a34a" name="Forecast" strokeDasharray="5 5" strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
