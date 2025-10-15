import api from "./client";

// ---- Provider Analytics ----
export const getProviderAnalytics = async (filters = {}) => {
  try {
    const res = await api.post("/analytics/provider", filters);
    return res.data;
  } catch (err) {
    console.warn("⚠️ Using mock analytics data.");
    return mockProviderAnalytics(filters);
  }
};

// ---- Average Rating ----
export const getAverageRating = async () => {
  try {
    const res = await api.get("/feedback/average");
    return res.data.averageRating;
  } catch {
    return 4.6; // fallback
  }
};

// ---- Charts Overview ----
export const getAnalyticsOverview = async (filters = {}) => {
  try {
    const res = await api.post("/analytics/overview", filters);
    return res.data;
  } catch (err) {
    console.warn("⚠️ Fallback: mock analytics overview");
    return mockAnalyticsOverview(filters);
  }
};

// ---- Export Analytics ----
export const exportAnalytics = async (filters = {}) => {
  try {
    const res = await api.post("/analytics/export", filters, { responseType: "blob" });
    return res.data;
  } catch {
    console.warn("⚠️ Export fallback: mock CSV blob");
    return new Blob(["Provider,Latency,SLA\nOpenAI,2100,98%"], { type: "text/csv" });
  }
};

// ---- Mock Fallbacks ----
const mockProviderAnalytics = (filters) => ({
  filters,
  providers: [
    { name: "OpenAI", avgLatency: 2100, sla: "98%", runs: 430 },
    { name: "Claude", avgLatency: 2400, sla: "97%", runs: 320 },
    { name: "Gemini", avgLatency: 3600, sla: "94%", runs: 270 },
  ],
});

const mockAnalyticsOverview = (filters) => {
  const days = filters.dateRange === "30d" ? 30 : 7;
  const labels = Array.from({ length: days }, (_, i) => `Day ${i + 1}`);
  const chartData = labels.map((label) => ({
    label,
    openai: 2100 + Math.random() * 300,
    claude: 2400 + Math.random() * 300,
    gemini: 3500 + Math.random() * 600,
  }));

  const kpis = [
    { title: "SLA Compliance", value: "97.6%", trend: "+0.4%" },
    { title: "Avg Latency", value: "2,720 ms", trend: "-1.1%" },
    { title: "Total Runs", value: "1,210", trend: "+2.9%" },
    { title: "Anomalies", value: "3", trend: "-25%" },
  ];

  return { filters, kpis, charts: chartData };
};
