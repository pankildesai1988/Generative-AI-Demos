import ChatWindow from "../components/ChatWindow";

export default { title: "Components/ChatWindow", component: ChatWindow };

export const Empty = {
  args: {
    messages: [],
    onSend: (q) => console.log("Send:", q),
    loading: false,
    lastHistoryId: null,
    onClear: () => console.log("Clear"),
    title: "AI Assistant",
    placeholder: "Ask a question...",
  },
};

export const WithMessages = {
  args: {
    messages: [
      { role: "user", text: "What is RAG?" },
      { role: "assistant", text: "**RAG** (Retrieval-Augmented Generation) combines document retrieval with LLM generation to provide grounded, accurate answers." },
    ],
    onSend: (q) => console.log("Send:", q),
    loading: false,
    lastHistoryId: 42,
    onClear: () => console.log("Clear"),
    title: "Medical Assistant",
  },
};

export const Loading = {
  args: {
    messages: [{ role: "user", text: "What are the side effects?" }],
    onSend: () => {},
    loading: true,
    lastHistoryId: null,
    onClear: () => {},
    title: "AI Assistant",
  },
};
