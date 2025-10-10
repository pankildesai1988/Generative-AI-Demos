import { useState } from "react";
import { runRag } from "../api/client";
import FeedbackModal from "./FeedbackModal";

export default function Chat() {
  const [query, setQuery] = useState("");
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [showFeedback, setShowFeedback] = useState(false);
  const [lastHistoryId, setLastHistoryId] = useState(null);

  const sendMessage = async () => {
    if (!query.trim()) return;
    setLoading(true);
    setMessages([...messages, { role: "user", text: query }]);

    try {
      const res = await runRag({
        query,
        provider: "OpenAI",
        model: "gpt-4o-mini",
        promptStyle: "rag",
      });

      const answer = res.data.ragAnswer;
      const historyId = res.data.historyId; // ✅ Real backend ID

      setLastHistoryId(historyId);
      setMessages((prev) => [...prev, { role: "assistant", text: answer }]);
    } catch (err) {
      setMessages((prev) => [...prev, { role: "assistant", text: "Error fetching response." }]);
    } finally {
      setLoading(false);
      setQuery("");
    }
  };

  return (
    <div className="flex flex-col h-screen bg-gray-50 relative">
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.map((m, i) => (
          <div
            key={i}
            className={`p-3 rounded-2xl max-w-xl ${
              m.role === "user"
                ? "bg-blue-500 text-white self-end"
                : "bg-gray-200 text-gray-900"
            }`}
          >
            {m.text}
            {m.role === "assistant" && (
              <div className="text-right mt-2">
                <button
                  onClick={() => setShowFeedback(true)}
                  className="text-sm text-blue-600 underline hover:text-blue-800"
                >
                  Give Feedback
                </button>
              </div>
            )}
          </div>
        ))}

        {loading && <div className="text-sm text-gray-400">Thinking...</div>}
      </div>

      <div className="p-3 flex border-t bg-white">
        <input
          type="text"
          className="flex-1 border rounded-xl p-2 mr-2"
          placeholder="Ask something..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && sendMessage()}
        />
        <button
          onClick={sendMessage}
          className="bg-blue-600 text-white px-4 rounded-xl disabled:opacity-50"
          disabled={loading}
        >
          Send
        </button>
      </div>

      {showFeedback && (
        <FeedbackModal
          historyId={lastHistoryId}
          onClose={() => setShowFeedback(false)}
        />
      )}
    </div>
  );
}
