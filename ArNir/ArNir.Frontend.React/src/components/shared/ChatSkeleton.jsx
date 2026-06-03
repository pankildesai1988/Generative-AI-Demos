import Skeleton from "./Skeleton";

export default function ChatSkeleton() {
  return (
    <div className="flex flex-col gap-3 p-4">
      <div className="flex justify-end">
        <div className="w-2/3">
          <Skeleton variant="chat-bubble" />
        </div>
      </div>
      <div className="flex justify-start">
        <div className="w-3/4">
          <Skeleton variant="chat-bubble" />
        </div>
      </div>
      <div className="flex justify-end">
        <div className="w-1/2">
          <Skeleton variant="chat-bubble" />
        </div>
      </div>
    </div>
  );
}
