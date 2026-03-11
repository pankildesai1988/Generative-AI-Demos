import {
  ResponsiveContainer,
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
} from "recharts";
import ReactMarkdown from "react-markdown";

/**
 * Displays analytics visualization or AI text insights.
 */
export default function InsightChartCard({ chart }) {
  if (!chart || !chart.data?.length)
    return <p className="text-gray-400 italic">No chart data available.</p>;

  const type = chart.type?.toLowerCase() ?? "text";

  const renderChart = () => {
    if (type.includes("bar")) {
      return (
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chart.data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="label" />
            <YAxis />
            <Tooltip />
            <Bar dataKey="value" fill="#2563eb" />
          </BarChart>
        </ResponsiveContainer>
      );
    }

    if (type.includes("line")) {
      return (
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chart.data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="label" />
            <YAxis />
            <Tooltip />
            <Line type="monotone" dataKey="value" stroke="#10b981" />
          </LineChart>
        </ResponsiveContainer>
      );
    }

    return (
      <div className="bg-gray-50 border rounded-lg p-4">
        {chart.data.map((d, i) => (
          <div key={i} className="mb-2">
            <strong>{d.label}:</strong>{" "}
            <span className="text-gray-700">{d.description || d.value}</span>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className="bg-white border rounded-xl shadow-sm p-4">
      <h4 className="font-medium text-gray-800 mb-2">{chart.title}</h4>
      {renderChart()}
      {type.includes("text") && chart.data[0]?.description && (
        <div className="mt-3 prose prose-sm text-gray-700">
          <ReactMarkdown>{chart.data[0].description}</ReactMarkdown>
        </div>
      )}
    </div>
  );
}
