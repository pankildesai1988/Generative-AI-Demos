import { useState, useCallback } from "react";
import { runRag } from "../api/rag";

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
  } = config;

  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [lastHistoryId, setLastHistoryId] = useState(null);
  const [chunks, setChunks] = useState([]);
  const [error, setError] = useState(null);

  const sendMessage = useCallback(
    async (query) => {
      if (!query.trim()) return;
      setLoading(true);
      setError(null);
      setMessages((prev) => [...prev, { role: "user", text: query }]);

      try {
        const res = await runRag({
          query,
          provider,
          model,
          promptStyle,
          topK,
          useHybrid,
        });

        const { ragAnswer, retrievedChunks, historyId } = res.data;
        setLastHistoryId(historyId);
        setChunks(retrievedChunks || []);
        setMessages((prev) => [
          ...prev,
          { role: "assistant", text: ragAnswer, chunks: retrievedChunks },
        ]);
      } catch (err) {
        const errorMsg =
          err.response?.data?.message || "Failed to get a response. Please try again.";
        setError(errorMsg);
        setMessages((prev) => [
          ...prev,
          { role: "assistant", text: errorMsg, isError: true },
        ]);
      } finally {
        setLoading(false);
      }
    },
    [provider, model, promptStyle, topK, useHybrid]
  );

  const clearChat = useCallback(() => {
    setMessages([]);
    setChunks([]);
    setLastHistoryId(null);
    setError(null);
  }, []);

  return { messages, sendMessage, loading, lastHistoryId, chunks, error, clearChat };
}
