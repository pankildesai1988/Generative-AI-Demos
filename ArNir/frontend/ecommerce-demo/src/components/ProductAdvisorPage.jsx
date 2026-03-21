import { useChat, ChatWindow } from "@arnir/shared";
import RecommendationList from "./RecommendationList";

export default function ProductAdvisorPage() {
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
          title="Product Advisor"
          placeholder="What kind of laptop are you looking for? Budget? Use case?"
        />
      </div>

      {/* Recommendations Panel */}
      <div className="hidden lg:block w-96 border-l dark:border-gray-700 bg-white dark:bg-gray-900 overflow-y-auto p-4">
        <RecommendationList chunks={chat.chunks} />
      </div>
    </div>
  );
}
