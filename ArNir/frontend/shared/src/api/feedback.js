import api from "./client";

export const submitFeedback = (feedback) => api.post("/feedback", feedback);
export const getFeedbacks = () => api.get("/feedback");
export const getAverageRating = () => api.get("/feedback/average");
