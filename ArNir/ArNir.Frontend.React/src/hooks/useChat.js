import { useState } from "react";
import { runRag } from "../api/rag";

export function useChat(defaultOptions = {}) {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [lastHistoryId, setLastHistoryId] = useState(null);

  const sendMessage = async (query, options = {}) => {
    if (!query.trim()) return;
    setLoading(true);
    setMessages((prev) => [...prev, { role: "user", text: query }]);

    try {
      const res = await runRag({
        query,
        provider: "OpenAI",
        model: "gpt-4o-mini",
        promptStyle: "rag",
        ...defaultOptions,
        ...options,
      });

      const answer = res.data.ragAnswer ?? res.data.answer ?? "No response.";
      const historyId = res.data.historyId ?? null;
      const chunks = res.data.chunks ?? res.data.retrievedChunks ?? [];

      setLastHistoryId(historyId);
      setMessages((prev) => [
        ...prev,
        { role: "assistant", text: answer, historyId, chunks },
      ]);
    } catch {
      setMessages((prev) => [
        ...prev,
        { role: "assistant", text: "Error fetching response.", isError: true },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const clearMessages = () => {
    setMessages([]);
    setLastHistoryId(null);
  };

  return { messages, loading, lastHistoryId, sendMessage, clearMessages };
}
