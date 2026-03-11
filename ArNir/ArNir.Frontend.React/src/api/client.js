import axios from "axios";

const api = axios.create({
  baseURL: "https://genaiapi.empiricaledge.site/api", // change in production
  headers: { "Content-Type": "application/json" },
});

// ---- RAG ----
export const runRag = (payload) => api.post("/rag/run", payload);
export const testRetrieval = (query) => api.post("/retrieval/test", JSON.stringify(query));

// ---- Feedback ----
export const submitFeedback = (feedback) => api.post("/feedback", feedback);
export const getFeedbacks = () => api.get("/feedback");
export const getAverageRating = () => api.get("/feedback/average");

// ---- Insights ----
export const getInsights = (payload) => api.post("/insights/analyze", payload);
export const getPredictions = (payload) => api.post("/insights/predict", payload);
export const getReport = (payload) => api.post("/insights/report", payload);

export default api;
