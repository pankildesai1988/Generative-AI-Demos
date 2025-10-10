import api from "./client";

export const getProviderAnalytics = async () => {
  const res = await api.get("/analytics/provider");
  return res.data;
};

export const getAverageRating = async () => {
  const res = await api.get("/feedback/average");
  return res.data.averageRating;
};
