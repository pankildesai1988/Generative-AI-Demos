import { useEffect, useState } from "react";
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from "recharts";
import { getAnalyticsOverview } from "../../api/analytics";
import { Loader, ErrorBanner } from "../shared";

export default function AnalyticsCharts({ filters }) {
  const [chartData, setChartData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activeProviders, setActiveProviders] = useState(
    filters?.provider === "All"
      ? ["OpenAI", "Claude", "Gemini"]
      : [filters?.provider]
  );

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError("");
      try {
        const data = await getAnalyticsOverview(filters);
        setChartData(data.charts || []);
      } catch (err) {
        console.error(err);
        setError("❌ Failed to load analytics chart data.");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [filters]);

  const toggleProvider = (provider) => {
    setActiveProviders((prev) =>
      prev.includes(provider)
        ? prev.filter((p) => p !== provider)
        : [...prev, provider]
    );
  };

  // --- UI rendering ---
  if (loading) return <Loader message="Loading analytics trends..." />;
  if (error) return <ErrorBanner message={error} onRetry={() => window.location.reload()} />;
  if (!chartData.length)
    return <p className="text-gray-600 mt-4">No analytics data available.</p>;

  return (
    <div id="analytics-charts" className="bg-white border rounded shadow-sm p-4 mt-4">
      <div className="flex justify-between items-center mb-3">
        <h3 className="font-semibold text-gray-700">Provider Performance Trends</h3>
        <div className="flex gap-2">
          {["OpenAI", "Claude", "Gemini"].map((provider) => (
            <button
              key={provider}
              onClick={() => toggleProvider(provider)}
              className={`text-xs font-medium px-2 py-1 rounded ${
                activeProviders.includes(provider)
                  ? "bg-blue-600 text-white"
                  : "bg-gray-200 text-gray-700"
              }`}
            >
              {provider}
            </button>
          ))}
        </div>
      </div>

      <ResponsiveContainer width="100%" height={320}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="label" />
          <YAxis />
          <Tooltip />
          <Legend />

          {activeProviders.includes("OpenAI") && (
            <Line
              type="monotone"
              dataKey="openai"
              stroke="#2563eb"
              name="OpenAI"
              strokeWidth={2}
              dot={false}
            />
          )}
          {activeProviders.includes("Claude") && (
            <Line
              type="monotone"
              dataKey="claude"
              stroke="#f59e0b"
              name="Claude"
              strokeWidth={2}
              dot={false}
            />
          )}
          {activeProviders.includes("Gemini") && (
            <Line
              type="monotone"
              dataKey="gemini"
              stroke="#10b981"
              name="Gemini"
              strokeWidth={2}
              dot={false}
            />
          )}
        </LineChart>
      </ResponsiveContainer>

      <p className="text-sm text-gray-500 mt-2">
        Viewing data for:{" "}
        <span className="font-medium">{activeProviders.join(", ")}</span>
      </p>
    </div>
  );
}
