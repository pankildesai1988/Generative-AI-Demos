import Skeleton from "../components/Skeleton";
import ChatSkeleton from "../components/ChatSkeleton";
import CardSkeleton from "../components/CardSkeleton";

export default { title: "Components/Skeleton" };

export const TextLines = { render: () => <Skeleton variant="text" count={3} /> };
export const Circle = { render: () => <Skeleton variant="circle" /> };
export const CardVariant = { render: () => <Skeleton variant="card" /> };
export const ChatBubble = { render: () => <Skeleton variant="chat-bubble" count={2} /> };
export const ChatLayout = { render: () => <ChatSkeleton /> };
export const CardLayout = { render: () => <CardSkeleton lines={4} /> };
