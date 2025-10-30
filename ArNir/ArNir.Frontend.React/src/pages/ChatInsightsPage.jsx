import { ChatInsightBox, InsightChartCard, SessionSidebar } from "../components/chat";
export default function ChatInsightsPage() {
  return (
    <div className="flex h-screen">
      <SessionSidebar />
      <div className="flex-1 flex flex-col">
        <ChatInsightBox />
        <InsightChartCard />
      </div>
    </div>
  );
}
