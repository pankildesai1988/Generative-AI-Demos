import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";
import { extractHighlightTerms } from "../../utils/highlightTerms";
import { normalizeMath } from "../../utils/normalizeMath";

const toneClasses = {
  entities: "bg-sky-50 text-sky-700 dark:bg-sky-900/30 dark:text-sky-300",
  numbers: "bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300",
  code: "bg-violet-50 text-violet-700 dark:bg-violet-900/30 dark:text-violet-300 font-mono",
};

export default function HighlightedMessage({ message }) {
  const isAssistant = message.role === "assistant";
  const terms = isAssistant ? extractHighlightTerms(message.text) : [];

  return (
    <div
      className={`flex flex-col ${message.role === "user" ? "items-end" : "items-start"} gap-2`}
    >
      <div
        className={`p-3 rounded-2xl max-w-[80%] text-sm ${
          message.role === "user"
            ? "bg-blue-600 text-white"
            : message.isError
            ? "bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 border border-red-200 dark:border-red-800"
            : "bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 border border-gray-200 dark:border-gray-700 shadow-sm"
        }`}
      >
        {isAssistant ? (
          <div className="prose prose-sm dark:prose-invert max-w-none">
            <ReactMarkdown remarkPlugins={[remarkMath]} rehypePlugins={[rehypeKatex]}>
              {normalizeMath(message.text || " ")}
            </ReactMarkdown>
          </div>
        ) : (
          message.text
        )}
      </div>

      {terms.length > 0 && (
        <div className="flex flex-wrap gap-1.5 ml-1 max-w-[80%]">
          {terms.slice(0, 12).map((term) => (
            <span
              key={`${term.category}:${term.label}`}
              className={`inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-medium ${toneClasses[term.category]}`}
            >
              {term.label}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
