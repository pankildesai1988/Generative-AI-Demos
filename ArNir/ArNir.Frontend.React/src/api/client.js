import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:5001/api", // 👈 Change when deployed
  headers: { "Content-Type": "application/json" },
});

// ---- RAG ----
export const runRag = (payload) => api.post("/rag/run", payload);
export const testRetrieval = (query) => api.post("/retrieval/test", JSON.stringify(query));

// ---- Feedback ----
export const submitFeedback = (feedback) => api.post("/feedback", feedback);
export const getFeedbacks = () => api.get("/feedback");

// ---- Analytics ----
export const getAnalyticsSummary = () => api.get("/analytics/provider");

export default api;
