import api from "./client";
import { getRuntimeApiUrl } from "../config/runtime";

export const runRag = (payload) => api.post("/rag/run", payload);

export const testRetrieval = (query) =>
  api.post("/retrieval/test", JSON.stringify(query));

export function buildStreamUrl(payload) {
  const base = getRuntimeApiUrl();
  const normalized = base.endsWith("/") ? base : `${base}/`;
  const url = new URL("rag/stream", normalized);

  const append = (key, value) => {
    if (value === undefined || value === null || value === "") return;
    url.searchParams.append(key, String(value));
  };

  append("query", payload.query);
  append("topK", payload.topK);
  append("useHybrid", payload.useHybrid);
  append("promptStyle", payload.promptStyle);
  append("saveAsNew", payload.saveAsNew ?? true);
  append("provider", payload.provider);
  append("model", payload.model);
  (payload.documentIds || []).forEach((id) => append("documentIds", id));

  return url.toString();
}
