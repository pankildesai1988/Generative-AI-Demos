import { useState } from "react";
import { sendChatQuery } from "../../api/chat";
import InsightChartCard from "./InsightChartCard";

export default function ChatInsightBox() {
  const [query, setQuery] = useState("");
  const [response, setResponse] = useState(null);

  const handleSend = async () => {
    const res = await sendChatQuery({ userQuery: query });
    setResponse(res.data);
  };

  return (
    <div className="p-4 flex flex-col">
      <textarea
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Ask a question about SLA or latency..."
        className="border rounded-lg p-2 mb-2 w-full"
      />
      <button onClick={handleSend} className="bg-blue-600 text-white px-4 py-2 rounded-lg">
        Ask
      </button>
      {response && <InsightChartCard response={response} />}
    </div>
  );
}
