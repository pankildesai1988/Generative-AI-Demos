import { createContext, useContext, useState, useEffect, useMemo } from "react";
import { themes } from "./themes";

const ThemeContext = createContext({ ...themes.healthcare, mode: "light", toggleMode: () => {} });

export function ThemeProvider({ demoType = "healthcare", children }) {
  const base = themes[demoType] || themes.healthcare;

  const [mode, setMode] = useState(() => {
    if (typeof window === "undefined") return "light";
    return localStorage.getItem("arnir-theme-mode") || "light";
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

  const toggleMode = () => setMode((prev) => (prev === "light" ? "dark" : "light"));

  const value = useMemo(
    () => ({ ...base, mode, toggleMode }),
    [base, mode]
  );

  return (
    <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
  );
}

export function useTheme() {
  return useContext(ThemeContext);
}
