import api from "./client";
import type { AxiosResponse } from "axios";

export const sendChatQuery = (payload: Record<string, unknown>): Promise<AxiosResponse> =>
  api.post("/chat/query", payload);

export const getSessionContext = (id: string): Promise<AxiosResponse> =>
  api.get(`/chat/context/${id}`);
