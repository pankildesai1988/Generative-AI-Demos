import { useEffect, useMemo, useState } from "react";

const STORAGE_KEY = "arnir-finance-comparison-history";

function readStoredHistory() {
  if (typeof window === "undefined") return [];
  try {
    return JSON.parse(window.localStorage.getItem(STORAGE_KEY) || "[]");
  } catch {
    return [];
  }
}

export default function useComparisonHistory() {
  const [entries, setEntries] = useState(readStoredHistory);

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(entries));
  }, [entries]);

  const addEntry = (entry) => {
    setEntries((current) => {
      const next = [entry, ...current.filter((item) => item.id !== entry.id)];
      return next.slice(0, 12);
    });
  };

  const clearEntries = () => setEntries([]);

  const newestTwo = useMemo(() => entries.slice(0, 2), [entries]);

  return {
    entries,
    newestTwo,
    addEntry,
    clearEntries,
  };
}
