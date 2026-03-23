import { useCallback, useState } from "react";
import { streamRag } from "../api/ragStream";
import { executeRagQuery, getRagErrorMessage } from "./chatRequest";
import { trackEvent } from "../analytics/tracker";
import type { ChatConfig, ChatHookReturn, Message, RetrievedChunk } from "../types";

interface StreamMetadata {
  historyId?: string;
  retrievedChunks?: RetrievedChunk[];
}

function replaceMessage(
  messages: Message[],
  messageId: string,
  updater: (message: Message) => Message,
): Message[] {
  return messages.map((message) =>
    message.id === messageId ? updater(message) : message
  );
}

export default function useChatStream(config: ChatConfig = {}): ChatHookReturn {
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
      // eslint-disable-next-line prefer-const -- assigned inside stream callback
      let metadata: StreamMetadata | null = null as StreamMetadata | null;

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
              const text = (payload as { text?: string })?.text ?? "";
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
              metadata = payload as StreamMetadata;
              setLastHistoryId((payload as StreamMetadata)?.historyId ?? null);
              setChunks((payload as StreamMetadata)?.retrievedChunks || []);
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
          message: (streamErr as Error).message,
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
              role: "assistant" as const,
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
