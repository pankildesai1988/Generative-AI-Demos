import React, { useEffect, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import toast from "react-hot-toast";
import axios from "axios";
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, XAxis, YAxis, Tooltip } from "recharts";

export default function InsightFeed({ messages = [] }) {
  const feedRef = useRef(null);

  useEffect(() => {
    if (feedRef.current) feedRef.current.scrollTop = feedRef.current.scrollHeight;

    const latest = messages[messages.length - 1];
    if (latest?.isError) toast.error(latest.text || "⚠️ Something went wrong.");
  }, [messages]);

  const handleAction = async (action) => {
    try {
      const { data } = await axios.post("/api/intelligence/action", { action });
      toast.success(`✅ ${action} executed successfully.`);
      console.log("Action result:", data);
    } catch (err) {
      console.error("Action execution failed:", err);
      toast.error(`❌ Failed to execute: ${action}`);
    }
  };

  const renderKPI = (data) => {
    if (!Array.isArray(data)) return null;
    const numericItems = data.filter((d) => typeof d.value === "number");
    if (numericItems.length === 0) return null;

    return (
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 mt-3">
        {numericItems.map((item, i) => (
          <motion.div
            key={i}
            whileHover={{ scale: 1.03 }}
            className="bg-blue-50 border border-blue-100 rounded-xl p-3 text-center shadow-sm"
          >
            <div className="text-gray-600 text-xs">{item.label}</div>
            <div className="text-2xl font-semibold text-blue-700">{item.value}</div>
          </motion.div>
        ))}
      </div>
    );
  };

  const renderChart = (chart) => {
    if (!chart?.data || !Array.isArray(chart.data) || chart.data.length === 0) return null;
    const numericData = chart.data.filter((d) => typeof d.value === "number");

    return (
      <div className="bg-gray-50 border border-gray-100 rounded-xl mt-4 p-3">
        <div className="text-sm font-medium text-gray-600 mb-2">{chart.title || "AI Visualization"}</div>
        <ResponsiveContainer width="100%" height={150}>
          {chart.type === "bar" ? (
            <BarChart data={numericData}>
              <XAxis dataKey="label" hide />
              <YAxis hide />
              <Tooltip />
              <Bar dataKey="value" fill="#3b82f6" radius={[6, 6, 0, 0]} />
            </BarChart>
          ) : (
            <LineChart data={numericData}>
              <XAxis dataKey="label" hide />
              <YAxis hide />
              <Tooltip />
              <Line type="monotone" dataKey="value" stroke="#3b82f6" strokeWidth={2} dot={false} />
            </LineChart>
          )}
        </ResponsiveContainer>
      </div>
    );
  };

  return (
    <div ref={feedRef} className="flex flex-col gap-4 overflow-y-auto max-h-[calc(100vh-220px)] p-2">
      <AnimatePresence>
        {messages.length > 0 ? (
          messages.map((msg, idx) => (
            <motion.div
              key={idx}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.3 }}
              className={`p-4 rounded-2xl shadow-sm border ${
                msg.role === "system"
                  ? "bg-red-50 text-red-700 border-red-200 self-center text-center w-fit"
                  : msg.role === "user"
                  ? "bg-blue-50 text-gray-900 self-end max-w-[75%] border-blue-100"
                  : "bg-white text-gray-800 self-start max-w-[85%] border-gray-100"
              }`}
            >
              <div className="text-[15px] leading-relaxed whitespace-pre-line">{msg.text}</div>

              {msg.chart?.data && renderKPI(msg.chart.data)}
              {msg.chart && renderChart(msg.chart)}

              {msg.suggestedActions?.length > 0 && (
                <div className="flex flex-wrap gap-2 mt-3">
                  {msg.suggestedActions.map((action, i) => (
                    <motion.button
                      key={i}
                      onClick={() => handleAction(action)}
                      whileHover={{ scale: 1.05 }}
                      whileTap={{ scale: 0.95 }}
                      className="px-3 py-1 bg-blue-100 text-blue-700 text-sm rounded-lg hover:bg-blue-200 shadow-sm transition-all"
                    >
                      {action}
                    </motion.button>
                  ))}
                </div>
              )}
            </motion.div>
          ))
        ) : (
          <motion.div
            className="text-sm text-gray-400 italic text-center mt-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
          >
            Start a conversation to see insights here...
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
