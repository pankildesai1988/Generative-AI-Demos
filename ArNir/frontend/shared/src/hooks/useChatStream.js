import { useCallback, useState } from "react";
import { streamRag } from "../api/ragStream";
import { executeRagQuery, getRagErrorMessage } from "./chatRequest";
import { trackEvent } from "../analytics/tracker";

function replaceMessage(messages, messageId, updater) {
  return messages.map((message) =>
    message.id === messageId ? updater(message) : message
  );
}

export default function useChatStream(config = {}) {
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

      const documentIds = options.documentIds ?? defaultDocumentIds;
      const assistantMessageId = `assistant-${Date.now()}-${Math.random().toString(16).slice(2)}`;

      setLoading(true);
      setError(null);
      setMessages((prev) => [
        ...prev,
        { role: "user", text: query },
        { id: assistantMessageId, role: "assistant", text: "", isStreaming: true },
      ]);

      trackEvent("chat", "submit", promptStyle, {
        provider,
        model,
        streaming: true,
        documentCount: documentIds.length,
      });

      let streamedText = "";
      let metadata = null;

      try {
        await streamRag(
          {
            query,
            provider,
            model,
            promptStyle,
            topK,
            useHybrid,
            saveAsNew: true,
            documentIds,
          },
          {
            onToken: (payload) => {
              const text = payload?.text ?? "";
              streamedText += text;
              setMessages((prev) =>
                replaceMessage(prev, assistantMessageId, (message) => ({
                  ...message,
                  text: streamedText,
                  isStreaming: true,
                }))
              );
            },
            onMetadata: (payload) => {
              metadata = payload;
              setLastHistoryId(payload?.historyId ?? null);
              setChunks(payload?.retrievedChunks || []);
            },
          }
        );

        setMessages((prev) =>
          replaceMessage(prev, assistantMessageId, (message) => ({
            ...message,
            text: streamedText,
            isStreaming: false,
            chunks: metadata?.retrievedChunks || [],
          }))
        );

        trackEvent("chat", "success", promptStyle, {
          provider,
          model,
          streaming: true,
          historyId: metadata?.historyId ?? null,
          chunkCount: metadata?.retrievedChunks?.length ?? 0,
        });
      } catch (streamErr) {
        setMessages((prev) =>
          prev.filter((message) => message.id !== assistantMessageId)
        );

        trackEvent("chat", "error", promptStyle, {
          provider,
          model,
          streaming: true,
          stage: "stream",
          message: streamErr.message,
        });

        try {
          const fallbackResult = await executeRagQuery({
            query,
            provider,
            model,
            promptStyle,
            topK,
            useHybrid,
            documentIds,
          });

          setLastHistoryId(fallbackResult.historyId);
          setChunks(fallbackResult.retrievedChunks);
          setMessages((prev) => [
            ...prev,
            {
              role: "assistant",
              text: fallbackResult.ragAnswer,
              chunks: fallbackResult.retrievedChunks,
            },
          ]);

          trackEvent("chat", "success", promptStyle, {
            provider,
            model,
            streaming: false,
            fallback: true,
            historyId: fallbackResult.historyId,
            chunkCount: fallbackResult.retrievedChunks.length,
          });
        } catch (fallbackErr) {
          const errorMsg = getRagErrorMessage(fallbackErr);
          setError(errorMsg);
          setMessages((prev) => [
            ...prev,
            { role: "assistant", text: errorMsg, isError: true },
          ]);

          trackEvent("chat", "error", promptStyle, {
            provider,
            model,
            streaming: false,
            fallback: true,
            message: errorMsg,
          });
        }
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
