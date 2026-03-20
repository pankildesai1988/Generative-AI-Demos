/**
 * Pre-composed skeleton mimicking the ChatWindow layout during initial load.
 */
export default function ChatSkeleton() {
  return (
    <div className="flex flex-col h-full bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm animate-pulse">
      {/* Header skeleton */}
      <div className="px-4 py-3 border-b dark:border-gray-700 bg-gray-50 dark:bg-gray-800 rounded-t-xl">
        <div className="h-5 w-48 bg-gray-200 dark:bg-gray-700 rounded" />
      </div>

      {/* Messages skeleton */}
      <div className="flex-1 p-4 space-y-4">
        {/* User message */}
        <div className="flex justify-end">
          <div className="h-10 w-52 bg-gray-200 dark:bg-gray-700 rounded-2xl" />
        </div>
        {/* Assistant message */}
        <div className="flex justify-start">
          <div className="space-y-2 w-72">
            <div className="h-10 bg-gray-200 dark:bg-gray-700 rounded-2xl" />
            <div className="h-4 w-40 bg-gray-100 dark:bg-gray-800 rounded" />
          </div>
        </div>
        {/* Another exchange */}
        <div className="flex justify-end">
          <div className="h-10 w-40 bg-gray-200 dark:bg-gray-700 rounded-2xl" />
        </div>
        <div className="flex justify-start">
          <div className="space-y-2 w-64">
            <div className="h-16 bg-gray-200 dark:bg-gray-700 rounded-2xl" />
            <div className="h-4 w-32 bg-gray-100 dark:bg-gray-800 rounded" />
          </div>
        </div>
      </div>

      {/* Input skeleton */}
      <div className="p-3 border-t dark:border-gray-700 bg-gray-50 dark:bg-gray-800 rounded-b-xl">
        <div className="flex items-center gap-2">
          <div className="flex-1 h-10 bg-gray-200 dark:bg-gray-700 rounded-xl" />
          <div className="h-10 w-10 bg-gray-200 dark:bg-gray-700 rounded-xl" />
        </div>
      </div>
    </div>
  );
}
