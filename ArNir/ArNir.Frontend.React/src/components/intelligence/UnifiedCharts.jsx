import React from "react";
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Area,
  Legend,
} from "recharts";

export default function UnifiedCharts({ charts = [] }) {
  if (!charts?.length) {
    return (
      <div className="p-6 text-center text-gray-400 italic">
        No chart data available.
      </div>
    );
  }

  const formatDate = (dateString) => {
    try {
      return new Date(dateString).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });
    } catch {
      return dateString;
    }
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {charts.map((chart, index) => {
        const hasForecast = chart.data.some((d) => d.predicted !== null);
        const hasLatency = chart.data.some((d) => d.avgLatency !== null);
        const hasBounds = chart.data.some(
          (d) => d.lowerBound !== null && d.upperBound !== null
        );

        const valueKey = hasForecast
          ? "predicted"
          : hasLatency
          ? "avgLatency"
          : "slaValue";

        return (
          <div
            key={index}
            className="bg-white rounded-2xl shadow p-4 border border-gray-100"
          >
            <h2 className="text-lg font-semibold text-gray-800 mb-3">
              {chart.title}
            </h2>
            <ResponsiveContainer width="100%" height={280}>
              <LineChart data={chart.data}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="date"
                  tickFormatter={formatDate}
                  stroke="#9ca3af"
                />
                <YAxis stroke="#9ca3af" />
                <Tooltip
                  formatter={(value, name) => [
                    value?.toLocaleString(undefined, {
                      maximumFractionDigits: 2,
                    }),
                    name === "predicted"
                      ? "Predicted Latency"
                      : name === "avgLatency"
                      ? "Avg Latency"
                      : "SLA (%)",
                  ]}
                  labelFormatter={(label) =>
                    `Date: ${formatDate(label)}`
                  }
                />
                <Legend />

                {/* Confidence interval shading for forecasts */}
                {hasBounds && (
                  <Area
                    type="monotone"
                    dataKey="upperBound"
                    stroke="none"
                    fill="rgba(59,130,246,0.15)"
                    isAnimationActive={false}
                    activeDot={false}
                  />
                )}

                {/* Main line */}
                <Line
                  type="monotone"
                  dataKey={valueKey}
                  stroke={hasForecast ? "#3b82f6" : "#10b981"}
                  strokeWidth={2}
                  dot={false}
                  activeDot={{ r: 4 }}
                />

                {/* Optional: overlay avg latency if both exist */}
                {hasForecast && hasLatency && (
                  <Line
                    type="monotone"
                    dataKey="avgLatency"
                    stroke="#a855f7"
                    strokeDasharray="4 4"
                    strokeWidth={2}
                    dot={false}
                  />
                )}
              </LineChart>
            </ResponsiveContainer>
          </div>
        );
      })}
    </div>
  );
}
