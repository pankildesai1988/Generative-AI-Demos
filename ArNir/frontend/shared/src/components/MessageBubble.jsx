import ReactMarkdown from "react-markdown";

export default function MessageBubble({ role, text, isError = false }) {
  const isUser = role === "user";

  return (
    <div className={`flex ${isUser ? "justify-end" : "justify-start"}`}>
      <div
        className={`max-w-2xl px-4 py-3 rounded-2xl ${
          isUser
            ? "bg-primary-600 text-white"
            : isError
            ? "bg-red-100 text-red-700 border border-red-200"
            : "bg-gray-100 text-gray-900"
        }`}
      >
        {isUser ? (
          <p>{text}</p>
        ) : (
          <div className="prose prose-sm max-w-none">
            <ReactMarkdown>{text}</ReactMarkdown>
          </div>
        )}
      </div>
    </div>
  );
}
