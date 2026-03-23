import { runRag } from "../api/rag";
import type { RagPayload, RagQueryResult, RetrievedChunk } from "../types";

export async function executeRagQuery({
  query,
  provider,
  model,
  promptStyle,
  topK,
  useHybrid,
  documentIds,
}: RagPayload): Promise<RagQueryResult> {
  const res = await runRag({
    query,
    provider,
    model,
    promptStyle,
    topK,
    useHybrid,
    documentIds,
  });

  const { ragAnswer, retrievedChunks, historyId } = res.data;

  return {
    ragAnswer,
    retrievedChunks: (retrievedChunks || []) as RetrievedChunk[],
    historyId,
  };
}

export function getRagErrorMessage(err: unknown): string {
  const axiosErr = err as { response?: { data?: { message?: string } } };
  return axiosErr.response?.data?.message || "Failed to get a response. Please try again.";
}
