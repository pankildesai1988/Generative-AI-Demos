import api from "./client";

export const runRag = (payload) => api.post("/rag/run", payload);
export const testRetrieval = (query) => api.post("/retrieval/test", JSON.stringify(query));
