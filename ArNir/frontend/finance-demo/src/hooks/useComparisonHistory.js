import { useCallback, useEffect, useMemo, useState } from "react";

const STORAGE_KEY = "arnir-finance-comparison-history";

function readStoredHistory() {
  if (typeof window === "undefined") return [];
  try {
    const raw = JSON.parse(window.localStorage.getItem(STORAGE_KEY) || "[]");
    // Deduplicate by id — stale localStorage may contain duplicates from old bug
    const seen = new Set();
    return raw.filter((entry) => {
      if (seen.has(entry.id)) return false;
      seen.add(entry.id);
      return true;
    });
  } catch {
    return [];
  }
}

export default function useComparisonHistory() {
  const [entries, setEntries] = useState(readStoredHistory);

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(entries));
  }, [entries]);

  const addEntry = useCallback((entry) => {
    setEntries((current) => {
      const next = [entry, ...current.filter((item) => item.id !== entry.id)];
      return next.slice(0, 12);
    });
  }, []);

  const clearEntries = useCallback(() => setEntries([]), []);

  const newestTwo = useMemo(() => entries.slice(0, 2), [entries]);

  return {
    entries,
    newestTwo,
    addEntry,
    clearEntries,
  };
}
