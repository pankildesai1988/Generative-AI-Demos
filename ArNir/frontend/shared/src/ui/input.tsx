import type { InputProps } from "../types";

export function Input({ className = "", ...props }: InputProps): React.ReactElement {
  return (
    <input
      {...props}
      aria-disabled={props.disabled || undefined}
      className={`border dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 rounded-lg px-3 py-2 w-full text-sm outline-none focus:ring-2 focus:ring-primary-300 dark:focus:ring-primary-600 placeholder:text-gray-400 dark:placeholder:text-gray-500 ${className}`}
    />
  );
}
