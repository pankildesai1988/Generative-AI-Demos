import { useState } from "react";
import { postChatPrompt } from "../api/intelligence";
import {
  InsightChatBox,
  InsightFeed,
} from "../components/intelligence";
import SemanticRecallPanel from "../components/intelligence/SemanticRecallPanel";
import Loader from "../components/shared/Loader";

export default function InsightChatPage() {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [lastPrompt, setLastPrompt] = useState("");

  const handleSend = async (prompt) => {
    setLastPrompt(prompt);
    setLoading(true);
    try {
      const data = await postChatPrompt(prompt);
      const responseText =
        data.responseText || data.content || "No response.";
      const chart = data.chart || data.Chart || null;

      setMessages((prev) => [
        ...prev,
        { role: "user", text: prompt },
        { role: "assistant", text: responseText, chart },
      ]);
    } catch (err) {
      console.error("Chat failed:", err);
      setMessages((prev) => [
        ...prev,
        { role: "assistant", text: "⚠️ Error connecting to AI service." },
      ]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-row">
      {/* Chat Area */}
      <div className="flex-1 flex flex-col p-6">
        <h2 className="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
          💬 Intelligence Assistant
        </h2>
        <InsightFeed messages={messages} loading={loading} />
        {loading && <Loader message="AI is analyzing your query..." />}
        <InsightChatBox onSend={handleSend} disabled={loading} />
      </div>

      {/* Semantic Recall Sidebar */}
      <SemanticRecallPanel lastQuery={lastPrompt} />
    </div>
  );
}
