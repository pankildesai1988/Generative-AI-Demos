import MessageBubble from "../components/MessageBubble";

export default { title: "Components/MessageBubble", component: MessageBubble };

export const UserMessage = { args: { role: "user", text: "What is RAG?" } };
export const AssistantMessage = { args: { role: "assistant", text: "**RAG** stands for Retrieval-Augmented Generation. It combines document retrieval with LLM generation." } };
export const ErrorMessage = { args: { role: "assistant", text: "Something went wrong.", isError: true } };
