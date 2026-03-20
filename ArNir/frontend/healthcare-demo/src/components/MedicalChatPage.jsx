import { useChat, ChatWindow, SourceViewer } from "@arnir/shared";
import SourceCitationPanel from "./SourceCitationPanel";

export default function MedicalChatPage() {
  const chat = useChat({
    provider: "OpenAI",
    model: "gpt-4o-mini",
    promptStyle: "rag",
    topK: 5,
  });

  return (
    <div className="flex h-full">
      {/* Chat Panel */}
      <div className="flex-1 flex flex-col p-4">
        <ChatWindow
          messages={chat.messages}
          onSend={chat.sendMessage}
          loading={chat.loading}
          lastHistoryId={chat.lastHistoryId}
          onClear={chat.clearChat}
          title="Medical Knowledge Assistant"
          placeholder="Ask about symptoms, treatments, drug interactions..."
        />
      </div>

      {/* Source Citations Panel */}
      <div className="w-96 border-l bg-white overflow-y-auto p-4">
        <SourceCitationPanel chunks={chat.chunks} />
      </div>
    </div>
  );
}
