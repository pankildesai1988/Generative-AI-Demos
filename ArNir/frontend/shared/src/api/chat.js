import api from "./client";

export const sendChatQuery = (payload) => api.post("/chat/query", payload);
export const getSessionContext = (id) => api.get(`/chat/context/${id}`);
