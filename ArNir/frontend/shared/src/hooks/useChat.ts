import { useState, useCallback } from "react";
import { executeRagQuery, getRagErrorMessage } from "./chatRequest";
import { trackEvent } from "../analytics/tracker";
import type { ChatConfig, ChatHookReturn, Message, RetrievedChunk } from "../types";

export default function useChat(config: ChatConfig = {}): ChatHookReturn {
  const {
    provider = "OpenAI",
    model = "gpt-4o-mini",
    promptStyle = "rag",
    topK = 5,
    useHybrid = false,
    documentIds: defaultDocumentIds = [],
  } = config;

  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [lastHistoryId, setLastHistoryId] = useState<string | null>(null);
  const [chunks, setChunks] = useState<RetrievedChunk[]>([]);
  const [error, setError] = useState<string | null>(null);

  const sendMessage = useCallback(
    async (query: string, options: { documentIds?: string[] } = {}) => {
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
