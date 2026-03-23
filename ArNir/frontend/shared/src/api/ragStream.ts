import api from "./client";
import type { RagPayload, StreamHandlers, SseEvent } from "../types";

function ensureTrailingSlash(url: string | undefined): string {
  const s = url ?? "";
  return s.endsWith("/") ? s : `${s}/`;
}

function buildStreamUrl(payload: RagPayload): string {
  const url = new URL("rag/stream", ensureTrailingSlash(api.defaults.baseURL));

  const appendValue = (key: string, value: unknown): void => {
    if (value === undefined || value === null || value === "") return;
    url.searchParams.append(key, String(value));
  };

  appendValue("query", payload.query);
  appendValue("topK", payload.topK);
  appendValue("useHybrid", payload.useHybrid);
  appendValue("promptStyle", payload.promptStyle);
  appendValue("saveAsNew", payload.saveAsNew ?? true);
  appendValue("provider", payload.provider);
  appendValue("model", payload.model);

  (payload.documentIds || []).forEach((documentId) =>
    appendValue("documentIds", documentId)
  );

  return url.toString();
}

export function parseSseEventBlock(block: string): SseEvent | null {
  const lines = block
    .split(/\r?\n/)
    .map((line) => line.trimEnd())
    .filter(Boolean);

  if (lines.length === 0) return null;

  let event = "message";
  const dataLines: string[] = [];

  for (const line of lines) {
    if (line.startsWith("event:")) {
      event = line.slice("event:".length).trim();
    } else if (line.startsWith("data:")) {
      dataLines.push(line.slice("data:".length).trim());
    }
  }

  const rawData = dataLines.join("\n");
  let data: unknown = rawData;

  if (rawData) {
    try {
      data = JSON.parse(rawData);
    } catch {
      data = rawData;
    }
  }

  return { event, data };
}

export async function streamRag(payload: RagPayload, handlers: StreamHandlers = {}): Promise<void> {
  const {
    onToken,
    onMetadata,
    onComplete,
    onError,
    signal,
  } = handlers;

  const response = await fetch(buildStreamUrl(payload), {
    method: "GET",
    headers: {
      Accept: "text/event-stream",
    },
    signal,
  });

  if (!response.ok || !response.body) {
    throw new Error(`Streaming request failed with status ${response.status}.`);
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { value, done } = await reader.read();
    buffer += decoder.decode(value || new Uint8Array(), { stream: !done });

    const blocks = buffer.split(/\r?\n\r?\n/);
    buffer = blocks.pop() ?? "";

    for (const block of blocks) {
      const parsed = parseSseEventBlock(block);
      if (!parsed) continue;

      if (parsed.event === "token") {
        onToken?.(parsed.data as { token: string });
      } else if (parsed.event === "metadata") {
        onMetadata?.(parsed.data as Parameters<NonNullable<StreamHandlers["onMetadata"]>>[0]);
      } else if (parsed.event === "complete") {
        onComplete?.(parsed.data as Record<string, unknown>);
      } else if (parsed.event === "error") {
        const errData = parsed.data as Record<string, string> | string;
        const error = new Error(
          (typeof errData === "object" ? errData?.message : errData) || "Streaming request failed."
        );
        onError?.(error);
        throw error;
      }
    }

    if (done) break;
  }

  if (buffer.trim()) {
    const parsed = parseSseEventBlock(buffer);
    if (parsed?.event === "complete") {
      onComplete?.(parsed.data as Record<string, unknown>);
    }
  }
}
