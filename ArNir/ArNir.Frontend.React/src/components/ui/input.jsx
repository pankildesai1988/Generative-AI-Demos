import React from "react";

export function Input({ className = "", ...props }) {
  return (
    <input
      {...props}
      className={`border rounded-lg px-3 py-2 w-full text-sm outline-none focus:ring-2 focus:ring-blue-300 ${className}`}
    />
  );
}
