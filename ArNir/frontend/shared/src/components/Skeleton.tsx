import type { SkeletonProps } from "../types";
import clsx from "clsx";

/**
 * Configurable skeleton loader with animate-pulse.
 * Variants: text (default), circle, card, chat-bubble.
 */
export default function Skeleton({ variant = "text", className = "", count = 1 }: SkeletonProps): React.ReactElement {
  const base = "animate-pulse bg-gray-200 dark:bg-gray-700 rounded";

  const variants: Record<string, string> = {
    text: "h-4 w-full rounded-md",
    circle: "h-10 w-10 rounded-full",
    card: "h-24 w-full rounded-xl",
    "chat-bubble": "h-12 rounded-2xl",
  };

  const items = Array.from({ length: count }, (_, i) => i);

  return (
    <>
      {items.map((i: number) => (
        <div key={i} className={clsx(base, variants[variant ?? "text"], className)} />
      ))}
    </>
  );
}
