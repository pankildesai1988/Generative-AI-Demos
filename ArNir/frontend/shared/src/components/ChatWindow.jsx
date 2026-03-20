import { useState, useRef, useEffect } from "react";
import { Send, Trash2 } from "lucide-react";
import MessageBubble from "./MessageBubble";
import TypingIndicator from "./TypingIndicator";
import FeedbackModal from "./FeedbackModal";

/**
 * Shared ChatWindow component for all demo frontends.
 * Uses the useChat hook externally — receives messages, sendMessage, loading via props.
 */
export default function ChatWindow({
  messages,
  onSend,
  loading,
  lastHistoryId,
  onClear,
  placeholder = "Ask a question...",
  title = "AI Assistant",
}) {
  const [query, setQuery] = useState("");
  const [showFeedback, setShowFeedback] = useState(false);
  const messagesEndRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  const handleSend = () => {
    if (!query.trim() || loading) return;
    onSend(query);
    setQuery("");
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-xl border border-gray-200 shadow-sm">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b bg-primary-50 rounded-t-xl">
        <h2 className="text-lg font-semibold text-primary-800">{title}</h2>
        <div className="flex items-center gap-2">
          {messages.length > 0 && (
            <button
              onClick={onClear}
              className="p-1.5 text-gray-400 hover:text-red-500 transition"
              title="Clear chat"
            >
              <Trash2 size={18} />
            </button>
          )}
        </div>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.length === 0 && (
          <div className="flex items-center justify-center h-full text-gray-400 text-sm">
            Start a conversation by typing a question below.
          </div>
        )}
        {messages.map((m, i) => (
          <div key={i}>
            <MessageBubble role={m.role} text={m.text} isError={m.isError} />
            {m.role === "assistant" && !m.isError && (
              <div className="flex justify-start mt-1 ml-1">
                <button
                  onClick={() => setShowFeedback(true)}
                  className="text-xs text-gray-400 hover:text-primary-600 transition"
                >
                  Rate this response
                </button>
              </div>
            )}
          </div>
        ))}
        {loading && <TypingIndicator />}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <div className="p-3 border-t bg-gray-50 rounded-b-xl">
        <div className="flex items-center gap-2">
          <input
            type="text"
            className="flex-1 border border-gray-300 rounded-xl px-4 py-2.5 text-sm outline-none focus:ring-2 focus:ring-primary-300 focus:border-primary-400 transition"
            placeholder={placeholder}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSend()}
            disabled={loading}
          />
          <button
            onClick={handleSend}
            disabled={loading || !query.trim()}
            className="bg-primary-600 text-white p-2.5 rounded-xl hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            <Send size={18} />
          </button>
        </div>
      </div>

      {/* Feedback Modal */}
      {showFeedback && (
        <FeedbackModal
          historyId={lastHistoryId}
          onClose={() => setShowFeedback(false)}
        />
      )}
    </div>
  );
}
