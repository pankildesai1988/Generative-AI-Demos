import type { ErrorBannerProps } from "../types";

export default function ErrorBanner({ message, onRetry }: ErrorBannerProps): React.ReactElement | null {
  if (!message) return null;
  return (
    <div role="alert" className="mt-4 bg-red-100 dark:bg-red-900/30 border border-red-300 dark:border-red-800 text-red-700 dark:text-red-400 p-3 rounded flex justify-between items-center">
      <span>{message}</span>
      {onRetry && (
        <button
          onClick={onRetry}
          className="text-sm font-semibold text-red-700 dark:text-red-400 hover:underline ml-3"
        >
          Retry
        </button>
      )}
    </div>
  );
}
