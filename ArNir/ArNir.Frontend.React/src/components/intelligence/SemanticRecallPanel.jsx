import React, { useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Lightbulb, RefreshCcw } from "lucide-react";
import toast from "react-hot-toast";
import { getRelatedInsights } from "../../api/intelligence";  // ✅ existing API helper
import Loader from "../shared/Loader";

export default function SemanticRecallPanel({ currentPrompt }) {
  const [relatedInsights, setRelatedInsights] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  const fetchRelatedInsights = async () => {
    if (!currentPrompt || currentPrompt.trim().length < 3) return;

    try {
      setIsLoading(true);
      const { data } = await getRelatedInsights({ query: currentPrompt });

      if (Array.isArray(data) && data.length > 0) {
        setRelatedInsights(data);
      } else {
        setRelatedInsights([]);
      }
    } catch (err) {
      console.error("Error fetching related insights:", err);
      toast.error("⚠️ Unable to fetch related insights.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const delay = setTimeout(fetchRelatedInsights, 800);
    return () => clearTimeout(delay);
  }, [currentPrompt]);

  return (
    <div className="flex flex-col h-full p-4 bg-gray-50 overflow-y-auto rounded-r-2xl">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-semibold text-gray-700 flex items-center gap-2">
          <Lightbulb size={16} className="text-yellow-500" />
          Semantic Recall
        </h3>
        <motion.button
          whileTap={{ rotate: 180 }}
          onClick={fetchRelatedInsights}
          className="p-1 rounded-md text-gray-500 hover:text-blue-600 transition-all"
          title="Refresh"
        >
          <RefreshCcw size={16} />
        </motion.button>
      </div>

      {isLoading ? (
        <div className="flex-1 flex justify-center items-center py-6">
          <Loader text="Fetching related insights..." />
        </div>
      ) : relatedInsights.length > 0 ? (
        <AnimatePresence>
          {relatedInsights.map((item, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="bg-white border border-gray-100 rounded-xl shadow-sm p-3 mb-2 cursor-pointer hover:bg-blue-50 transition-all"
            >
              <div className="text-[13px] text-gray-700 line-clamp-3">
                {item.userQuery || item.query || "Related insight"}
              </div>
              <div className="text-xs text-gray-400 mt-1">
                {item.createdAt ? new Date(item.createdAt).toLocaleString() : "Historical"}
              </div>
            </motion.div>
          ))}
        </AnimatePresence>
      ) : (
        <div className="text-xs text-gray-400 italic mt-4 text-center">
          No related insights found yet.
        </div>
      )}
    </div>
  );
}
