import { renderHook, act, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import useChatStream from "../hooks/useChatStream";

const streamRagMock = vi.fn();
const executeRagQueryMock = vi.fn();
const trackEventMock = vi.fn();

vi.mock("../api/ragStream", () => ({
  streamRag: (...args) => streamRagMock(...args),
}));

vi.mock("../hooks/chatRequest", () => ({
  executeRagQuery: (...args) => executeRagQueryMock(...args),
  getRagErrorMessage: (err) =>
    err?.response?.data?.message || err?.message || "Unknown error",
}));

vi.mock("../analytics/tracker", () => ({
  trackEvent: (...args) => trackEventMock(...args),
}));

describe("useChatStream", () => {
  beforeEach(() => {
    streamRagMock.mockReset();
    executeRagQueryMock.mockReset();
    trackEventMock.mockReset();
  });

  it("updates the assistant message progressively while streaming", async () => {
    streamRagMock.mockImplementation(async (_, handlers) => {
      handlers.onToken?.({ text: "Hello " });
      handlers.onToken?.({ text: "world" });
      handlers.onMetadata?.({
        historyId: 17,
        retrievedChunks: [{ documentId: 1, chunkText: "Hello world" }],
      });
      handlers.onComplete?.({ done: true });
    });

    const { result } = renderHook(() => useChatStream());

    await act(async () => {
      await result.current.sendMessage("Explain revenue");
    });

    await waitFor(() =>
      expect(result.current.messages).toHaveLength(2)
    );

    expect(result.current.messages[0]).toMatchObject({
      role: "user",
      text: "Explain revenue",
    });
    expect(result.current.messages[1]).toMatchObject({
      role: "assistant",
      text: "Hello world",
      isStreaming: false,
      chunks: [{ documentId: 1, chunkText: "Hello world" }],
    });
    expect(result.current.lastHistoryId).toBe(17);
    expect(result.current.chunks).toEqual([
      { documentId: 1, chunkText: "Hello world" },
    ]);
    expect(trackEventMock).toHaveBeenCalledWith(
      "chat",
      "submit",
      "rag",
      expect.objectContaining({ streaming: true })
    );
    expect(trackEventMock).toHaveBeenCalledWith(
      "chat",
      "success",
      "rag",
      expect.objectContaining({ streaming: true, historyId: 17 })
    );
  });

  it("falls back to the non-streaming request when SSE fails", async () => {
    streamRagMock.mockRejectedValue(new Error("sse unavailable"));
    executeRagQueryMock.mockResolvedValue({
      ragAnswer: "Fallback answer",
      historyId: 23,
      retrievedChunks: [{ documentId: 2, chunkText: "Fallback answer" }],
    });

    const { result } = renderHook(() => useChatStream());

    await act(async () => {
      await result.current.sendMessage("Need fallback");
    });

    await waitFor(() =>
      expect(result.current.messages).toHaveLength(2)
    );

    expect(result.current.messages[1]).toMatchObject({
      role: "assistant",
      text: "Fallback answer",
      chunks: [{ documentId: 2, chunkText: "Fallback answer" }],
    });
    expect(result.current.lastHistoryId).toBe(23);
    expect(trackEventMock).toHaveBeenCalledWith(
      "chat",
      "error",
      "rag",
      expect.objectContaining({ streaming: true, stage: "stream" })
    );
    expect(trackEventMock).toHaveBeenCalledWith(
      "chat",
      "success",
      "rag",
      expect.objectContaining({ streaming: false, fallback: true, historyId: 23 })
    );
  });
});
