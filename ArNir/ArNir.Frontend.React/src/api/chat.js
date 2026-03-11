import client from "./client";

export const sendChatQuery = (payload) => client.post("/chat/query", payload);
export const getSessionContext = (id) => client.get(`/chat/context/${id}`);
