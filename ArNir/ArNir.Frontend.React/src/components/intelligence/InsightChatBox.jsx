import React, { useState, useEffect, useRef } from "react";
import { motion } from "framer-motion";
import { SendHorizonal } from "lucide-react";
import toast from "react-hot-toast";
import { postChatPrompt } from "../../api/intelligence";  // ✅ existing API helper
import InsightFeed from "./InsightFeed";
import Loader from "../shared/Loader";
import SemanticRecallPanel from "./SemanticRecallPanel";

export default function InsightChatBox() {
  const [messages, setMessages] = useState([]);
  const [userInput, setUserInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [sessionId, setSessionId] = useState(() => localStorage.getItem("sessionId") || crypto.randomUUID());
  const chatEndRef = useRef(null);

  useEffect(() => {
    localStorage.setItem("sessionId", sessionId);
  }, [sessionId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!userInput.trim()) return;

    const newMsg = { role: "user", text: userInput };
    setMessages((prev) => [...prev, newMsg]);
    setIsLoading(true);

    try {
      const { data } = await postChatPrompt({
        sessionId,
        prompt: userInput,
      });

      const aiMsg = {
        role: "assistant",
        text: data.responseText || "No insights generated.",
        chart: data.chart || null,
        insightSummary: data.insightSummary || "",
        suggestedActions: data.suggestedActions || [],
        isError: data.isError || false,
      };

      setMessages((prev) => [...prev, aiMsg]);
      setUserInput("");
    } catch (err) {
      console.error("Chat error:", err);
      toast.error("⚠️ Failed to get response from the server.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (chatEndRef.current) chatEndRef.current.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  return (
    <div className="flex h-[calc(100vh-100px)]">
      {/* Left Chat Column */}
      <div className="flex-1 flex flex-col bg-white rounded-2xl shadow-lg border border-gray-100">
        <div className="flex items-center justify-between p-4 border-b border-gray-100 bg-gray-50 rounded-t-2xl">
          <h2 className="text-lg font-semibold text-gray-700">Intelligence Chat</h2>
          <span className="text-xs text-gray-400">Session: {sessionId.slice(0, 8)}</span>
        </div>

        {/* Chat Feed */}
        <div className="flex-1 overflow-y-auto p-4">
          <InsightFeed messages={messages} />
          {isLoading && (
            <div className="flex justify-center py-6">
              <Loader text="Generating insight..." />
            </div>
          )}
          <div ref={chatEndRef} />
        </div>

        {/* Chat Input */}
        <form
          onSubmit={handleSubmit}
          className="p-4 border-t border-gray-100 bg-gray-50 flex items-center gap-2 rounded-b-2xl"
        >
          <input
            type="text"
            className="flex-1 rounded-xl border border-gray-200 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Ask something like 'Compare SLA trends for OpenAI vs Gemini'..."
            value={userInput}
            onChange={(e) => setUserInput(e.target.value)}
          />
          <motion.button
            whileHover={{ scale: 1.1 }}
            whileTap={{ scale: 0.95 }}
            type="submit"
            disabled={isLoading}
            className={`p-2 rounded-xl ${
              isLoading ? "bg-gray-200 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"
            } text-white shadow-sm transition-all`}
          >
            <SendHorizonal size={20} />
          </motion.button>
        </form>
      </div>

      {/* Right Sidebar — Semantic Recall Panel */}
      <div className="w-[320px] border-l border-gray-100 bg-gray-50 rounded-r-2xl">
        <SemanticRecallPanel currentPrompt={userInput} />
      </div>
    </div>
  );
}
