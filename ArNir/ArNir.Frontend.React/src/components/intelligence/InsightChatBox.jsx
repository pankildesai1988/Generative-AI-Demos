import React, { useState, useEffect, useRef } from "react";
import ReactMarkdown from "react-markdown";
import { Button } from "../ui/button";
import { Input } from "../ui/input";
import { Loader } from "../shared/Loader";
import { Send } from "lucide-react";
import { motion } from "framer-motion";

export default function InsightChatBox({ onSubmit, response }) {
  const [message, setMessage] = useState("");
  const [isTyping, setIsTyping] = useState(false);
  const [displayedText, setDisplayedText] = useState("");
  const typingTimeout = useRef(null);

  // Typing animation effect
  useEffect(() => {
    if (response) {
      setIsTyping(true);
      setDisplayedText("");
      let index = 0;
      clearTimeout(typingTimeout.current);

      const typeNext = () => {
        if (index < response.length) {
          setDisplayedText((prev) => prev + response[index]);
          index++;
          typingTimeout.current = setTimeout(typeNext, 10);
        } else {
          setIsTyping(false);
        }
      };
      typeNext();
    }
  }, [response]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!message.trim()) return;
    onSubmit(message);
    setMessage("");
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <div className="w-full bg-white rounded-2xl border border-gray-100 shadow-sm p-5 flex flex-col space-y-4">
      <div className="flex-1 overflow-y-auto max-h-[280px] bg-gray-50 rounded-xl p-4 border border-gray-200">
        {!response && !isTyping && (
          <p className="text-gray-400 text-sm italic text-center">
            Ask a question to get AI-powered insights...
          </p>
        )}

        {isTyping && (
          <motion.div
            className="text-gray-600 text-sm font-medium"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
          >
            <span className="font-semibold text-blue-600">AI:</span>{" "}
            <span className="animate-pulse">Thinking...</span>
          </motion.div>
        )}

        {displayedText && (
          <div className="prose prose-sm max-w-none text-gray-800 mt-2">
            <ReactMarkdown>{displayedText}</ReactMarkdown>
          </div>
        )}
      </div>

      <form
        onSubmit={handleSubmit}
        className="flex items-center space-x-2 border-t pt-3"
      >
        <Input
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Ask about performance, anomalies, or forecast..."
          className="flex-1"
        />
        <Button
          type="submit"
          disabled={!message.trim()}
          className="flex items-center space-x-1"
        >
          <Send size={16} />
          <span>Send</span>
        </Button>
      </form>
    </div>
  );
}
