import { runRag } from "../api/rag";

export async function executeRagQuery({
  query,
  provider,
  model,
  promptStyle,
  topK,
  useHybrid,
  documentIds,
}) {
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
    retrievedChunks: retrievedChunks || [],
    historyId,
  };
}

export function getRagErrorMessage(err) {
  return err.response?.data?.message || "Failed to get a response. Please try again.";
}
