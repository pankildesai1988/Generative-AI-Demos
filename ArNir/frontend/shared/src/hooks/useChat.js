import { useState, useCallback } from "react";
import { executeRagQuery, getRagErrorMessage } from "./chatRequest";
import { trackEvent } from "../analytics/tracker";

/**
 * Shared chat hook for all demo frontends.
 * @param {Object} config - { provider, model, promptStyle, topK, useHybrid }
 */
export default function useChat(config = {}) {
  const {
    provider = "OpenAI",
    model = "gpt-4o-mini",
    promptStyle = "rag",
    topK = 5,
    useHybrid = false,
    documentIds: defaultDocumentIds = [],
  } = config;

  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [lastHistoryId, setLastHistoryId] = useState(null);
  const [chunks, setChunks] = useState([]);
  const [error, setError] = useState(null);

  const sendMessage = useCallback(
    async (query, options = {}) => {
      if (!query.trim()) return;
      setLoading(true);
      setError(null);
      setMessages((prev) => [...prev, { role: "user", text: query }]);

      try {
        const documentIds = options.documentIds ?? defaultDocumentIds;
        trackEvent("chat", "submit", promptStyle, {
          provider,
          model,
          streaming: false,
          documentCount: documentIds.length,
        });

        const result = await executeRagQuery({
          query,
          provider,
          model,
          promptStyle,
          topK,
          useHybrid,
          documentIds,
        });
        setLastHistoryId(result.historyId);
        setChunks(result.retrievedChunks);
        setMessages((prev) => [
          ...prev,
          { role: "assistant", text: result.ragAnswer, chunks: result.retrievedChunks },
        ]);
        trackEvent("chat", "success", promptStyle, {
          provider,
          model,
          streaming: false,
          historyId: result.historyId,
          chunkCount: result.retrievedChunks.length,
        });
      } catch (err) {
        const errorMsg = getRagErrorMessage(err);
        setError(errorMsg);
        setMessages((prev) => [
          ...prev,
          { role: "assistant", text: errorMsg, isError: true },
        ]);
        trackEvent("chat", "error", promptStyle, {
          provider,
          model,
          streaming: false,
          message: errorMsg,
        });
      } finally {
        setLoading(false);
      }
    },
    [provider, model, promptStyle, topK, useHybrid, defaultDocumentIds]
  );

  const clearChat = useCallback(() => {
    setMessages([]);
    setChunks([]);
    setLastHistoryId(null);
    setError(null);
  }, []);

  return { messages, sendMessage, loading, lastHistoryId, chunks, error, clearChat };
}
