import type { CardSkeletonProps } from "../types";

/**
 * Pre-composed skeleton mimicking a Card/panel layout during loading.
 */
export default function CardSkeleton({ lines = 3 }: CardSkeletonProps): React.ReactElement {
  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-sm p-4 animate-pulse">
      <div className="h-5 w-32 bg-gray-200 dark:bg-gray-700 rounded mb-3" />
      <div className="space-y-2">
        {Array.from({ length: lines }, (_, i: number) => (
          <div
            key={i}
            className="h-4 bg-gray-200 dark:bg-gray-700 rounded"
            style={{ width: `${85 - i * 15}%` }}
          />
        ))}
      </div>
    </div>
  );
}
