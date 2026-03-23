import api from "./client";
import type { AxiosResponse } from "axios";

interface FeedbackPayload {
  historyId: string;
  rating: number;
  comment?: string;
}

export const submitFeedback = (feedback: FeedbackPayload): Promise<AxiosResponse> =>
  api.post("/feedback", feedback);

export const getFeedbacks = (): Promise<AxiosResponse> => api.get("/feedback");

export const getAverageRating = (): Promise<AxiosResponse> => api.get("/feedback/average");
