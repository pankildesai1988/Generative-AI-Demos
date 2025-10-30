import React from "react";
import clsx from "clsx";

/**
 * ✅ Named exports for Card components
 * Works with: import { Card } from "../ui/card";
 */
export function Card({ className = "", children, ...props }) {
  return (
    <div
      className={clsx(
        "rounded-2xl border border-gray-200 bg-white shadow-sm p-4",
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
}

export function CardHeader({ children }) {
  return <div className="font-semibold mb-2">{children}</div>;
}

export function CardContent({ children }) {
  return <div className="text-sm text-gray-700">{children}</div>;
}
