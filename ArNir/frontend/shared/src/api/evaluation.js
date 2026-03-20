import api from "./client";

export const evaluate = (payload) => api.post("/evaluation/evaluate", payload);
export const getEvaluationHistory = (params) => api.get("/evaluation/history", { params });
export const getEvaluationStats = () => api.get("/evaluation/stats");
