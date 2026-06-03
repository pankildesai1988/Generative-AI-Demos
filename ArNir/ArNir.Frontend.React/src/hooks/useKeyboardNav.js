import { useEffect } from "react";

export default function useKeyboardNav(key, handler, enabled = true) {
  useEffect(() => {
    if (!enabled) return;
    const onKey = (e) => {
      if (e.key === key) handler(e);
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [key, handler, enabled]);
}
