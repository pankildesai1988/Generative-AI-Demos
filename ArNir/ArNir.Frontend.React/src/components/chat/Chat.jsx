import { useEffect, useRef, useState } from "react";
import { Send } from "lucide-react";
import { useChatStream } from "../../hooks/useChatStream";
import HighlightedMessage from "./HighlightedMessage";
import SourceDocPanel from "./SourceDocPanel";
import ExportChatButton from "./ExportChatButton";
import FeedbackModal from "../analytics/FeedbackModal";
import TypingIndicator from "../shared/TypingIndicator";

const confidenceBadgeClass = {
  high: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300",
  medium: "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300",
  low: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300",
};

export default function Chat() {
  const [query, setQuery] = useState("");
  const [feedbackId, setFeedbackId] = useState(null);
  const bottomRef = useRef(null);

  const {
    messages,
    loading,
    lastChunks,
    lastConfidence,
    sendMessage,
    clearMessages,
  } = useChatStream();

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  // Document Scope removed — every query runs against ALL uploaded documents.
  // Omitting documentIds makes the backend skip the filter (hasDocumentFilter = false).
  const handleSend = () => {
    sendMessage(query);
    setQuery("");
  };

  const showConfidence = lastConfidence && messages.some((m) => m.role === "assistant" && !m.isError);

  return (
    <div className="flex h-full flex-col gap-4 lg:flex-row bg-gray-50 dark:bg-gray-900">
      {/* Center — Chat */}
      <section className="flex min-h-0 flex-1 flex-col p-4">
        {/* Header */}
        <div className="mb-3 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
              ArNir Knowledge Assistant
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              Ask grounded questions across all uploaded documents.
            </p>
          </div>
          <div className="flex items-center gap-3">
            {showConfidence && (
              <span
                className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${confidenceBadgeClass[lastConfidence]}`}
              >
                {lastConfidence.toUpperCase()} confidence
              </span>
            )}
            <ExportChatButton messages={messages} selectedDocuments={[]} />
            {messages.length > 0 && (
              <button
                type="button"
                onClick={clearMessages}
                className="text-xs text-gray-500 hover:underline dark:text-gray-400"
              >
                Clear
              </button>
            )}
          </div>
        </div>

        {/* Message list */}
        <div className="flex-1 overflow-y-auto rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4 space-y-3">
          {messages.length === 0 ? (
            <div className="flex h-full items-center justify-center">
              <p className="text-sm text-gray-400 dark:text-gray-500">
                Ask anything about your documents…
              </p>
            </div>
          ) : (
            messages.map((m, i) => (
              <div key={i} className="space-y-1">
                <HighlightedMessage message={m} />
                {m.role === "assistant" && !m.streaming && m.historyId && (
                  <div className="text-right">
                    <button
                      onClick={() => setFeedbackId(m.historyId)}
                      className="text-xs text-blue-500 hover:underline"
                    >
                      Rate response
                    </button>
                  </div>
                )}
              </div>
            ))
          )}
          {loading && <TypingIndicator />}
          <div ref={bottomRef} />
        </div>

        {/* Input */}
        <div className="mt-3 flex gap-2">
          <input
            type="text"
            className="flex-1 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Ask about anything in your documents…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && !e.shiftKey && handleSend()}
          />
          <button
            onClick={handleSend}
            disabled={loading || !query.trim()}
            className="bg-blue-600 text-white p-2.5 rounded-xl disabled:opacity-50 hover:bg-blue-700 transition-colors"
            aria-label="Send"
          >
            <Send size={18} />
          </button>
        </div>
      </section>

      {/* Right — Source Documents */}
      <aside className="min-h-0 w-full border-t bg-white p-4 dark:border-gray-700 dark:bg-gray-900 lg:block lg:w-96 lg:border-l lg:border-t-0">
        <SourceDocPanel chunks={lastChunks} />
      </aside>

      {feedbackId !== null && (
        <FeedbackModal
          historyId={feedbackId}
          onClose={() => setFeedbackId(null)}
        />
      )}
    </div>
  );
}
