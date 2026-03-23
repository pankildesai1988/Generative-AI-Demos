import { createContext, useContext, useState, useEffect, useMemo } from "react";
import type { ThemeContextValue, DemoType } from "../types";
import { themes } from "./themes";

const ThemeContext = createContext<ThemeContextValue | null>(null);

interface ThemeProviderProps {
  demoType?: DemoType;
  children: React.ReactNode;
}

export function ThemeProvider({ demoType = "healthcare", children }: ThemeProviderProps): React.ReactElement {
  const base = themes[demoType] || themes.healthcare;

  const [mode, setMode] = useState<"light" | "dark">(() => {
    if (typeof window === "undefined") return "light";
    return (localStorage.getItem("arnir-theme-mode") as "light" | "dark") || "light";
  });

  useEffect(() => {
    const root = document.documentElement;
    if (mode === "dark") {
      root.classList.add("dark");
    } else {
      root.classList.remove("dark");
    }
    localStorage.setItem("arnir-theme-mode", mode);
  }, [mode]);

  const toggleMode = (): void => setMode((prev) => (prev === "light" ? "dark" : "light"));

  const value = useMemo<ThemeContextValue>(
    () => ({ ...base, mode, toggleMode }),
    [base, mode]
  );

  return (
    <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
  );
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return ctx;
}
