import { useEffect, useState } from "react";
import { AnalyticsCharts, KPIWidget, FiltersBar, ExportButton, FeedbackModal } from "./index";
import { Loader, ErrorBanner } from "../shared";
import { getAnalyticsOverview } from "../../api/analytics";

export default function AnalyticsDashboard() {
  const [analytics, setAnalytics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [filters, setFilters] = useState({ provider: "All", dateRange: "7d" });

  useEffect(() => {
    async function fetchAnalytics() {
      setLoading(true);
      setError("");
      try {
        const res = await getAnalyticsOverview(filters);
        setAnalytics(res.data);
      } catch (err) {
        console.error(err);
        setError("❌ Failed to fetch analytics data. Please try again later.");
      } finally {
        setLoading(false);
      }
    }
    fetchAnalytics();
  }, [filters]);

  if (loading) return <Loader message="Loading analytics data..." />;
  if (error) return <ErrorBanner message={error} onRetry={() => setFilters({ ...filters })} />;

  return (
    <div className="space-y-6">
      {/* Filters */}
      <FiltersBar filters={filters} setFilters={setFilters} />

      {/* KPIs Section */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {analytics?.kpis?.map((k, i) => (
          <KPIWidget key={i} title={k.title} value={k.value} trend={k.trend} />
        ))}
      </div>

      {/* Charts Section */}
      <AnalyticsCharts filters={filters} />

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 mt-4">
        <FeedbackModal />
        <ExportButton data={analytics} />
      </div>
    </div>
  );
}
