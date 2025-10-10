import { useEffect, useState } from "react";
import { getProviderAnalytics, getAverageRating } from "../api/analytics";
import KPIWidget from "./KPIWidget";
import AnalyticsCharts from "./AnalyticsCharts";
import ExportButton from "./ExportButton";
import FiltersBar from "./FiltersBar";
import { BarChart3, Timer, Star, Activity } from "lucide-react";

export default function AnalyticsDashboard() {
  const [analytics, setAnalytics] = useState([]);
  const [averageRating, setAverageRating] = useState(0);
  const [filters, setFilters] = useState({ provider: "", model: "" });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    const providerData = await getProviderAnalytics();
    const avgRating = await getAverageRating();
    setAnalytics(providerData.data || providerData);
    setAverageRating(avgRating);
  };

  const totalRuns = analytics.reduce((sum, x) => sum + x.totalRuns, 0);
  const avgLatency = analytics.reduce((sum, x) => sum + x.avgTotalLatencyMs, 0) / (analytics.length || 1);
  const avgSla = analytics.reduce((sum, x) => sum + x.slaComplianceRate, 0) / (analytics.length || 1);

  return (
    <div className="space-y-8">
      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <KPIWidget title="Total Runs" value={totalRuns.toFixed(0)} icon={<BarChart3 />} />
        <KPIWidget title="Avg Latency (ms)" value={avgLatency.toFixed(2)} icon={<Timer />} />
        <KPIWidget title="SLA Compliance (%)" value={avgSla.toFixed(1)} icon={<Activity />} />
        <KPIWidget title="Avg Rating" value={averageRating.toFixed(1)} icon={<Star />} />
      </div>

      {/* Filters */}
      <FiltersBar filters={filters} onChange={(e) => setFilters({ ...filters, [e.target.name]: e.target.value })} />

      {/* Charts */}
      <AnalyticsCharts data={analytics} />

      {/* Export */}
      <div className="flex justify-end">
        <ExportButton data={analytics} />
      </div>
    </div>
  );
}
