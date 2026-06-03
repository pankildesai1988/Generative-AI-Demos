const variants = {
  text: "h-4 w-full rounded-md",
  circle: "h-10 w-10 rounded-full",
  card: "h-24 w-full rounded-xl",
  "chat-bubble": "h-12 rounded-2xl",
};

export default function Skeleton({ variant = "text", className = "", count = 1 }) {
  const base = "animate-pulse bg-gray-200 dark:bg-gray-700 rounded";
  const shape = variants[variant] ?? variants.text;
  return (
    <>
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className={`${base} ${shape} ${className}`} />
      ))}
    </>
  );
}
