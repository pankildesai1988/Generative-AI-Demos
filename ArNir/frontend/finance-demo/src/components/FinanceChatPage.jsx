import { useChat, ChatWindow, SourceViewer } from "@arnir/shared";
import InsightsPanel from "./InsightsPanel";

export default function FinanceChatPage() {
  const chat = useChat({
    provider: "OpenAI",
    model: "gpt-4o-mini",
    promptStyle: "rag",
    topK: 5,
  });

  // Get the latest assistant message for insights extraction
  const lastAssistantMsg = [...chat.messages]
    .reverse()
    .find((m) => m.role === "assistant" && !m.isError);

  return (
    <div className="flex h-full">
      {/* Chat Panel */}
      <div className="flex-1 flex flex-col p-4">
        <div className="flex-1 mb-4">
          <ChatWindow
            messages={chat.messages}
            onSend={chat.sendMessage}
            loading={chat.loading}
            lastHistoryId={chat.lastHistoryId}
            onClear={chat.clearChat}
            title="Financial Document Analyzer"
            placeholder="Ask about revenue, risk factors, market trends..."
          />
        </div>

        {/* Source Documents (bottom panel) */}
        {chat.chunks.length > 0 && (
          <div className="h-48 overflow-y-auto bg-white rounded-xl border p-3">
            <SourceViewer chunks={chat.chunks} title="Source Documents" />
          </div>
        )}
      </div>

      {/* Insights Panel (right) */}
      <div className="w-96 border-l bg-white overflow-y-auto p-4">
        <InsightsPanel
          answer={lastAssistantMsg?.text || ""}
          hasData={chat.messages.length > 0}
        />
      </div>
    </div>
  );
}
