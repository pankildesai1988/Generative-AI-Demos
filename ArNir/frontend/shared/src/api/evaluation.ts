import api from "./client";
import type { AxiosResponse } from "axios";

export const evaluate = (payload: Record<string, unknown>): Promise<AxiosResponse> =>
  api.post("/evaluation/evaluate", payload);

export const getEvaluationHistory = (params?: Record<string, unknown>): Promise<AxiosResponse> =>
  api.get("/evaluation/history", { params });

export const getEvaluationStats = (): Promise<AxiosResponse> => api.get("/evaluation/stats");
