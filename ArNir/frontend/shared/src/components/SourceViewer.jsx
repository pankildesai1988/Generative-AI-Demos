import { useState } from "react";
import { ChevronDown, ChevronRight, FileText } from "lucide-react";

/**
 * Collapsible source viewer — displays retrievedChunks from RAG response.
 */
export default function SourceViewer({ chunks = [], title = "Sources" }) {
  const [expandedIndex, setExpandedIndex] = useState(null);

  if (!chunks || chunks.length === 0) return null;

  const toggle = (i) => setExpandedIndex(expandedIndex === i ? null : i);

  return (
    <div className="space-y-2">
      <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
        <FileText size={16} className="text-primary-600 dark:text-primary-400" />
        {title} ({chunks.length})
      </h3>
      <div className="space-y-1.5">
        {chunks.map((chunk, i) => (
          <div
            key={i}
            className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden"
          >
            <button
              onClick={() => toggle(i)}
              className="w-full flex items-center gap-2 px-3 py-2 bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-750 transition text-left"
            >
              {expandedIndex === i ? (
                <ChevronDown size={14} className="text-gray-500 dark:text-gray-400" />
              ) : (
                <ChevronRight size={14} className="text-gray-500 dark:text-gray-400" />
              )}
              <span className="text-xs font-medium text-primary-700 dark:text-primary-400">
                Source {i + 1}
              </span>
              {chunk.documentTitle && (
                <span className="text-xs text-gray-500 dark:text-gray-400 truncate flex-1">
                  — {chunk.documentTitle}
                </span>
              )}
              {chunk.rank && (
                <span className="text-xs bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 px-1.5 py-0.5 rounded">
                  #{chunk.rank}
                </span>
              )}
            </button>
            {expandedIndex === i && (
              <div className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-900 border-t dark:border-gray-700">
                <p className="whitespace-pre-wrap leading-relaxed">
                  {chunk.chunkText || chunk.text || chunk.content || "No content available."}
                </p>
                {chunk.retrievalType && (
                  <p className="text-xs text-gray-400 dark:text-gray-500 mt-2">
                    Retrieval: {chunk.retrievalType}
                  </p>
                )}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
