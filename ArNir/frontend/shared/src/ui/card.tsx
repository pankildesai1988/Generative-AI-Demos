import type { CardProps } from "../types";
import clsx from "clsx";

export function Card({ className = "", children, ...props }: CardProps): React.ReactElement {
  return (
    <div
      className={clsx(
        "rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-sm p-4",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

export function CardHeader({ children }: { children: React.ReactNode }): React.ReactElement {
  return <div className="font-semibold mb-2 dark:text-gray-100">{children}</div>;
}

export function CardContent({ children }: { children: React.ReactNode }): React.ReactElement {
  return <div className="text-sm text-gray-700 dark:text-gray-300">{children}</div>;
}
