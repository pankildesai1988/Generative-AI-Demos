import { useEffect } from "react";

/**
 * Calls a handler when a specific key is pressed.
 * Commonly used for Escape key to close modals/panels.
 */
export default function useKeyboardNav(key, handler, enabled = true) {
  useEffect(() => {
    if (!enabled) return;

    const handleKeyDown = (e) => {
      if (e.key === key) {
        handler(e);
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [key, handler, enabled]);
}
