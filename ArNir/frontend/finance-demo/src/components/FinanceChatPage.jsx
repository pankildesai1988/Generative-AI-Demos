import { useEffect, useMemo } from "react";
import { Link } from "react-router-dom";
import { useChatStream, ChatWindow, SourceViewer } from "@arnir/shared";
import { ArrowRightLeft } from "lucide-react";
import InsightsPanel from "./InsightsPanel";
import DataTable from "./DataTable";
import { extractMarkdownTable } from "../utils/tableExtractor";
import { extractChartData } from "../utils/chartDataExtractor";
import { scoreRisk } from "../utils/riskScorer";
import { useFinanceContext } from "./FinanceContext";

export default function FinanceChatPage() {
  const chat = useChatStream({
    provider: "OpenAI",
    model: "gpt-4o-mini",
    promptStyle: "rag",
    topK: 5,
  });

  // Get the latest completed assistant message for insights extraction
  const lastAssistantMsg = [...chat.messages]
    .reverse()
    .find((m) => m.role === "assistant" && !m.isError && !m.isStreaming);
  const table = useMemo(
    () => extractMarkdownTable(lastAssistantMsg?.text || ""),
    [lastAssistantMsg]
  );
  const { comparisonHistory } = useFinanceContext();

  useEffect(() => {
    if (!chat.lastHistoryId) return;
    const assistantMsg = [...chat.messages]
      .reverse()
      .find((m) => m.role === "assistant" && !m.isError && !m.isStreaming);
    if (!assistantMsg?.text) return;

    const entry = {
      id: `${chat.lastHistoryId}`,
      query:
        [...chat.messages].reverse().find((message) => message.role === "user")?.text ||
        "Analysis",
      answer: assistantMsg.text,
      createdAt: new Date().toISOString(),
      chartData: extractChartData(assistantMsg.text),
      risk: scoreRisk(assistantMsg.text),
    };

    comparisonHistory.addEntry(entry);
  // chat.messages is intentionally read inside but not listed as dep —
  // lastHistoryId only changes once per completed response, at which point
  // messages are stable. Listing messages would cause infinite re-renders.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [chat.lastHistoryId]);

  return (
    <div className="flex h-full">
      <div className="flex-1 flex flex-col p-4">
        <div className="mb-3 flex items-center justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Financial Document Analyzer
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              Ask about revenue, margins, risks, trends, and comparative performance.
            </p>
          </div>
          <Link
            to="/compare"
            className="inline-flex items-center gap-2 rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-200"
          >
            <ArrowRightLeft size={16} />
            Compare Mode
          </Link>
        </div>

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

        {table && (
          <div className="mb-4">
            <DataTable table={table} />
          </div>
        )}

        {chat.chunks.length > 0 && (
          <div className="h-48 overflow-y-auto bg-white dark:bg-gray-900 rounded-xl border dark:border-gray-700 p-3">
            <SourceViewer chunks={chat.chunks} title="Source Documents" />
          </div>
        )}
      </div>

      <div className="hidden lg:block w-96 border-l dark:border-gray-700 bg-white dark:bg-gray-900 overflow-y-auto p-4">
        <InsightsPanel
          answer={lastAssistantMsg?.text || ""}
          hasData={chat.messages.length > 0}
        />
      </div>
    </div>
  );
}
