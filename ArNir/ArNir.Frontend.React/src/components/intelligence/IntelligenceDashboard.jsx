import React, { useEffect, useState, useCallback } from "react";
import {
  getDashboardData,
  postChatPrompt,
  exportDashboardData,
} from "../../api/intelligence";
import {
  KPIGroup,
  UnifiedCharts,
  InsightChatBox,
  AlertList,
} from "./index";
import FiltersBar from "./FiltersBar";
import ExportPanel from "./ExportPanel";
import Loader from "../shared/Loader";
import InsightFeed from "./InsightFeed";

/**
 * Intelligence Dashboard
 * ------------------------
 * Unified AI Intelligence view with KPIs, charts, insights, alerts, and exports.
 * Reactively updates when filters change.
 */
export default function IntelligenceDashboard() {
  const [filters, setFilters] = useState({
    provider: "",
    startDate: "",
    endDate: "",
  });

  const [data, setData] = useState({
    kpis: [],
    charts: [],
    gptSummary: "",
    activeAlerts: [],
    aiInsights: [], // 🧠 NEW — predictive AI insights
  });

  const [loading, setLoading] = useState(false);
  const [insightResponse, setInsightResponse] = useState("");

  /**
   * Fetch dashboard data (KPI, charts, alerts, insights)
   */
  const loadDashboard = useCallback(async () => {
    try {
      setLoading(true);
      const result = await getDashboardData(filters);
      setData(result || {});
      console.log("✅ Dashboard refreshed with filters:", filters);
    } catch (err) {
      console.error("❌ Error loading dashboard:", err);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  /**
   * Auto-refresh when filters change (debounced)
   */
  useEffect(() => {
    const timeout = setTimeout(() => {
      loadDashboard();
    }, 400);
    return () => clearTimeout(timeout);
  }, [loadDashboard]);

  /**
   * Handle GPT chat prompt submission
   */
  const handlePrompt = async (prompt) => {
    try {
      const reply = await postChatPrompt(prompt);
      setInsightResponse(reply);
    } catch (err) {
      console.error("❌ Chat prompt failed:", err);
    }
  };

  /**
   * Handle export in CSV/Excel/PDF
   */
  const handleExport = async (format) => {
    try {
      await exportDashboardData(filters, format);
      console.log(`📁 Exported ${format} file successfully.`);
    } catch (err) {
      console.error("❌ Export failed:", err);
    }
  };

  if (loading)
    return (
      <div className="p-6">
        <Loader message="Loading intelligence data..." />
      </div>
    );

  return (
    <div className="p-6 grid gap-6">
      {/* --- Filters --- */}
      <FiltersBar filters={filters} onChange={setFilters} />

      {/* --- KPI Cards --- */}
      <KPIGroup kpis={data.kpis || []} />

      {/* --- Charts --- */}
      {data.charts && data.charts.length > 0 ? (
        <UnifiedCharts charts={data.charts} />
      ) : (
        <div className="text-gray-500 text-sm italic">
          No chart data available for selected filters.
        </div>
      )}

      {/* --- Alerts --- */}
      <AlertList alerts={data.activeAlerts || []} />

      {/* --- GPT Insight Summary --- */}
      {data.gptSummary && (
        <div className="rounded-xl border bg-gray-50 shadow-sm p-4 text-gray-800">
          <h4 className="font-semibold mb-2">AI Insight Summary</h4>
          <p>{data.gptSummary}</p>
        </div>
      )}

      {/* --- AI Predictive Insights Feed --- */}
      {data.aiInsights && (
        <InsightFeed
          insights={data.aiInsights}
          onRefresh={loadDashboard}
        />
      )}

      {/* --- Chat Box --- */}
      <InsightChatBox onSubmit={handlePrompt} response={insightResponse} />

      {/* --- Export Controls --- */}
      <ExportPanel onExport={handleExport} />
    </div>
  );
}
