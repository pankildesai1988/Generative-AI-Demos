import api from "./client";
import type { AxiosResponse } from "axios";
import type { RagPayload } from "../types";

export const runRag = (payload: RagPayload): Promise<AxiosResponse> =>
  api.post("/rag/run", payload);

export const testRetrieval = (query: string): Promise<AxiosResponse> =>
  api.post("/retrieval/test", JSON.stringify(query));
