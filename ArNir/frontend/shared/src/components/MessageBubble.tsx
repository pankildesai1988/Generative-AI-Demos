import type { MessageBubbleProps } from "../types";
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";

export default function MessageBubble({ role, text, isError = false }: MessageBubbleProps): React.ReactElement {
  const isUser = role === "user";

  return (
    <div className={`flex ${isUser ? "justify-end" : "justify-start"}`} role="article" aria-label={`${isUser ? "Your" : "Assistant"} message`}>
      <div
        className={`max-w-2xl px-4 py-3 rounded-2xl ${
          isUser
            ? "bg-primary-600 text-white"
            : isError
            ? "bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400 border border-red-200 dark:border-red-800"
            : "bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-gray-100"
        }`}
      >
        {isUser ? (
          <p>{text}</p>
        ) : (
          <div className="prose prose-sm dark:prose-invert max-w-none">
            <ReactMarkdown remarkPlugins={[remarkMath]} rehypePlugins={[rehypeKatex]}>
              {normalizeMath(text)}
            </ReactMarkdown>
          </div>
        )}
      </div>
    </div>
  );
}

/**
 * Coerces the loose math notation LLMs commonly emit into the `$$…$$` delimiters that
 * remark-math recognises, without disturbing already-delimited or non-math text:
 * - converts `\( … \)` / `\[ … \]` to `$…$` / `$$…$$`
 * - wraps a parenthesised line that is clearly a LaTeX formula (contains \frac, \sum,
 *   \text, =, _ or ^) in `$$…$$`
 * Lines with no LaTeX markers are returned untouched.
 */
function normalizeMath(text: string): string {
  if (!text) return text;

  let out = text
    .replace(/\\\[([\s\S]+?)\\\]/g, (_m, body) => `$$${body.trim()}$$`)
    .replace(/\\\(([\s\S]+?)\\\)/g, (_m, body) => `$${body.trim()}$`);

  const hasLatex = /\\(frac|sum|sqrt|text|alpha|beta|sigma|mu|theta|cdot|times|partial|int)\b|[_^]\{/;

  out = out
    .split("\n")
    .map((line) => {
      const trimmed = line.trim();
      // A whole line wrapped in ( … ) that contains LaTeX markers → display math.
      if (/^\(.+\)$/.test(trimmed) && hasLatex.test(trimmed) && !trimmed.includes("$")) {
        return `$$${trimmed.slice(1, -1).trim()}$$`;
      }
      return line;
    })
    .join("\n");

  return out;
}
