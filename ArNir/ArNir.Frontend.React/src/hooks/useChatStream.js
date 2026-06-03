import { useState, useCallback, useMemo } from "react";
import { streamRag } from "../api/ragStream";
import { runRag } from "../api/rag";
import { trackEvent } from "../analytics/tracker";

const ASSISTANT_PLACEHOLDER = { role: "assistant", text: "", streaming: true };

function deriveConfidence(chunks = []) {
  if (!chunks.length) return "low";
  const scores = chunks
    .map((c) => Number(c.score ?? c.Score ?? 0))
    .filter((s) => !Number.isNaN(s));
  const avg = scores.length ? scores.reduce((a, b) => a + b, 0) / scores.length : 0;
  if (chunks.length >= 3 && avg >= 0.75) return "high";
  if (chunks.length >= 1) return "medium";
  return "low";
}

export function useChatStream(defaultOptions = {}) {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [lastHistoryId, setLastHistoryId] = useState(null);
  const [lastChunks, setLastChunks] = useState([]);

  const lastConfidence = useMemo(() => deriveConfidence(lastChunks), [lastChunks]);

  const sendMessage = useCallback(
    async (query, options = {}) => {
      if (!query.trim()) return;
      setLoading(true);
      trackEvent("chat", "submit", null, { length: query.length });

      setMessages((prev) => [
        ...prev,
        { role: "user", text: query },
        { ...ASSISTANT_PLACEHOLDER },
      ]);

      const payload = {
        query,
        provider: "OpenAI",
        model: "gpt-4o-mini",
        promptStyle: "rag",
        ...defaultOptions,
        ...options,
      };

      try {
        let fullText = "";

        await streamRag(payload, {
          onToken: (data) => {
            const token =
              typeof data === "object"
                ? data?.token ?? data?.text ?? ""
                : String(data);
            fullText += token;
            setMessages((prev) => {
              const next = [...prev];
              const last = next[next.length - 1];
              if (last?.streaming) next[next.length - 1] = { ...last, text: fullText };
              return next;
            });
          },
          onMetadata: (data) => {
            const historyId = data?.historyId ?? null;
            const chunks = data?.chunks ?? data?.retrievedChunks ?? [];
            setLastHistoryId(historyId);
            setLastChunks(chunks);
            setMessages((prev) => {
              const next = [...prev];
              const last = next[next.length - 1];
              if (last?.streaming)
                next[next.length - 1] = {
                  ...last,
                  streaming: false,
                  historyId,
                  chunks,
                };
              return next;
            });
          },
          onComplete: () => {
            setMessages((prev) => {
              const next = [...prev];
              const last = next[next.length - 1];
              if (last?.streaming)
                next[next.length - 1] = { ...last, streaming: false };
              return next;
            });
            trackEvent("chat", "success", null, {});
          },
        });
      } catch {
        try {
          const res = await runRag(payload);
          const answer = res.data.ragAnswer ?? res.data.answer ?? "No response.";
          const historyId = res.data.historyId ?? null;
          const chunks = res.data.chunks ?? res.data.retrievedChunks ?? [];
          setLastHistoryId(historyId);
          setLastChunks(chunks);
          setMessages((prev) => {
            const next = [...prev];
            next[next.length - 1] = { role: "assistant", text: answer, historyId, chunks };
            return next;
          });
          trackEvent("chat", "success", "fallback-rest", {});
        } catch {
          setMessages((prev) => {
            const next = [...prev];
            next[next.length - 1] = {
              role: "assistant",
              text: "Error fetching response.",
              isError: true,
            };
            return next;
          });
          trackEvent("chat", "error", null, {});
        }
      } finally {
        setLoading(false);
      }
    },
    [defaultOptions]
  );

  const clearMessages = useCallback(() => {
    setMessages([]);
    setLastHistoryId(null);
    setLastChunks([]);
  }, []);

  return {
    messages,
    loading,
    lastHistoryId,
    lastChunks,
    lastConfidence,
    sendMessage,
    clearMessages,
  };
}
