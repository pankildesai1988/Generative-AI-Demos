
import client from "./client";

// Updated version that sends filters as query params
export const getDashboardData = async (filters = {}) => {
  try {
    const { data } = await client.get("/intelligence/dashboard", {
      params: {
        provider: filters.provider || "",
        startDate: filters.startDate || "",
        endDate: filters.endDate || "",
      },
    });
    return data;
  } catch (err) {
    console.warn("⚠️ Dashboard API failed, using mock data", err.message);
    return {
      kpis: [
        { label: "SLA Compliance", value: "98%", trend: "+1%" },
        { label: "Avg Latency", value: "312 ms", trend: "-3%" },
        { label: "Total Runs", value: "427", trend: "+5%" },
      ],
      charts: [
        {
          title: "Provider SLA Trends",
          data: [
            { date: "Mon", value: 96 },
            { date: "Tue", value: 98 },
            { date: "Wed", value: 97 },
            { date: "Thu", value: 99 },
            { date: "Fri", value: 98 },
          ],
        },
      ],
      activeAlerts: [
        { type: "Latency Spike", message: "Gemini exceeded 500ms." },
        { type: "SLA Drop", message: "Claude dropped below 95% SLA." },
      ],
    };
  }
};

// ✅ Export functionality (new)
export const exportDashboardData = async (filters, format) => {
  try {
    const response = await client.get("/intelligence/export", {
      params: { ...filters, format },
      responseType: "blob", // So browser treats it as a file
    });

    // Create a blob URL and trigger download
    const blob = new Blob([response.data], {
      type:
        format === "excel"
          ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
          : format === "csv"
          ? "text/csv"
          : "application/pdf",
    });

    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `intelligence_export_${format}_${new Date().toISOString().slice(0, 10)}.${format}`;
    a.click();
    window.URL.revokeObjectURL(url);

  } catch (err) {
    console.error("❌ Export failed:", err);
    alert("Export failed. Please check backend connection.");
  }
};

// --- Chat ---
export const postChatPrompt = (payload) =>
  client.post("/intelligence/chat", {
    sessionId: payload.sessionId,
    query: payload.prompt, // ✅ matches backend now
  });

export const getRelatedInsights = (payload) =>
  client.post("/intelligence/related", {
    query: payload.query || payload.prompt,
  });

//export const postChatPrompt = (payload) =>
//  client.post("/intelligence/chat", payload);

export const postActionIntent = (action) =>
  client.post("/intelligence/action", { action });

//export const fetchRelatedInsights = (payload) =>
//  client.post("/intelligence/related", payload);

//export const getRelatedInsights = fetchRelatedInsights; // ✅ alias
