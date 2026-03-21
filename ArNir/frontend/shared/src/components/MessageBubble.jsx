import ReactMarkdown from "react-markdown";

export default function MessageBubble({ role, text, isError = false }) {
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
            <ReactMarkdown>{text}</ReactMarkdown>
          </div>
        )}
      </div>
    </div>
  );
}
