import React from "react";

/**
 * InsightFeed.jsx
 * ----------------------------
 * Displays predictive AI insights from the backend.
 *
 * Props:
 *  - insights: Array<AIInsightDto>
 *  - onRefresh (optional): callback for reload
 */
export default function InsightFeed({ insights = [], onRefresh }) {
  if (!insights || insights.length === 0) {
    return (
      <div className="p-4 border rounded-lg bg-gray-50 text-gray-500 italic">
        No AI insights available for the selected filters.
      </div>
    );
  }

  const getSeverityStyle = (severity) => {
    switch (severity?.toLowerCase()) {
      case "critical":
        return "bg-red-50 border-red-300 text-red-800";
      case "warning":
        return "bg-yellow-50 border-yellow-300 text-yellow-800";
      default:
        return "bg-gray-50 border-gray-200 text-gray-800";
    }
  };

  return (
    <div className="rounded-xl border bg-white shadow p-4">
      <div className="flex justify-between items-center mb-3">
        <h4 className="font-semibold text-gray-800">Predictive AI Insights</h4>
        {onRefresh && (
          <button
            onClick={onRefresh}
            className="text-sm text-blue-600 hover:underline"
          >
            Refresh
          </button>
        )}
      </div>

      <div className="space-y-3">
        {insights.map((insight, i) => (
          <div
            key={i}
            className={`p-3 rounded-lg border shadow-sm ${getSeverityStyle(
              insight.severity
            )}`}
          >
            <div className="font-medium">{insight.title}</div>
            <div className="text-sm mt-1">{insight.insightText}</div>
            <div className="text-xs mt-2 text-gray-500">
              {new Date(insight.generatedAt).toLocaleString()}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
