export function Button({ children, className = "", variant = "primary", ...props }) {
  const base = "px-4 py-2 rounded-lg font-medium transition disabled:opacity-50 disabled:cursor-not-allowed";
  const variants = {
    primary: "bg-primary-600 text-white hover:bg-primary-700",
    secondary: "bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600",
    accent: "bg-accent-500 text-white hover:bg-accent-600",
    ghost: "text-primary-600 dark:text-primary-400 hover:bg-primary-50 dark:hover:bg-primary-900/20",
  };

  return (
    <button
      {...props}
      className={`${base} ${variants[variant] || variants.primary} ${className}`}
    >
      {children}
    </button>
  );
}
