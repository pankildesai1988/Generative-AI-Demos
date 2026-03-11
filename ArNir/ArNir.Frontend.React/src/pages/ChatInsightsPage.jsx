import React from "react";
import InsightChatBox from "../components/intelligence/InsightChatBox";
import SemanticRecallPanel from "../components/intelligence/SemanticRecallPanel";

export default function ChatInsightsPage() {
  return (
    <div className="grid grid-cols-4 gap-4 h-[calc(100vh-80px)] p-4">
      <div className="col-span-3 flex flex-col bg-white rounded-2xl shadow-lg p-4">
        <h2 className="text-lg font-semibold mb-3">Intelligence Chat</h2>
        <InsightChatBox />
      </div>

      <div className="col-span-1 bg-white rounded-2xl shadow-lg p-4">
        <SemanticRecallPanel />
      </div>
    </div>
  );
}
