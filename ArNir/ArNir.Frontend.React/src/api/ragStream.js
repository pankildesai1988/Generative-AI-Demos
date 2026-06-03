import { buildStreamUrl } from "./rag";

function parseSseBlock(block) {
  const lines = block
    .split(/\r?\n/)
    .map((l) => l.trimEnd())
    .filter(Boolean);

  if (!lines.length) return null;

  let event = "message";
  const dataLines = [];

  for (const line of lines) {
    if (line.startsWith("event:")) {
      event = line.slice("event:".length).trim();
    } else if (line.startsWith("data:")) {
      dataLines.push(line.slice("data:".length).trim());
    }
  }

  const raw = dataLines.join("\n");
  let data = raw;
  if (raw) {
    try {
      data = JSON.parse(raw);
    } catch {
      data = raw;
    }
  }

  return { event, data };
}

export async function streamRag(payload, handlers = {}) {
  const { onToken, onMetadata, onComplete, onError, signal } = handlers;

  const response = await fetch(buildStreamUrl(payload), {
    method: "GET",
    headers: { Accept: "text/event-stream" },
    signal,
  });

  if (!response.ok || !response.body) {
    throw new Error(`Stream request failed: ${response.status}`);
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
      const parsed = parseSseBlock(block);
      if (!parsed) continue;

      if (parsed.event === "token") {
        onToken?.(parsed.data);
      } else if (parsed.event === "metadata") {
        onMetadata?.(parsed.data);
      } else if (parsed.event === "complete") {
        onComplete?.(parsed.data);
      } else if (parsed.event === "error") {
        const msg =
          typeof parsed.data === "object"
            ? parsed.data?.message
            : parsed.data;
        const err = new Error(msg || "Streaming failed.");
        onError?.(err);
        throw err;
      }
    }

    if (done) break;
  }

  if (buffer.trim()) {
    const parsed = parseSseBlock(buffer);
    if (parsed?.event === "complete") onComplete?.(parsed.data);
  }
}
