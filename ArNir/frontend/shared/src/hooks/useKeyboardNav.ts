import { useEffect } from "react";

export default function useKeyboardNav(
  key: string,
  handler: (e: KeyboardEvent) => void,
  enabled: boolean = true,
): void {
  useEffect(() => {
    if (!enabled) return;

    const handleKeyDown = (e: KeyboardEvent): void => {
      if (e.key === key) {
        handler(e);
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [key, handler, enabled]);
}
