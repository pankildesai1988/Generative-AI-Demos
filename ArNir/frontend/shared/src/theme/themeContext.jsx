import { createContext, useContext } from "react";
import { themes } from "./themes";

const ThemeContext = createContext(themes.healthcare);

export function ThemeProvider({ demoType = "healthcare", children }) {
  const theme = themes[demoType] || themes.healthcare;
  return (
    <ThemeContext.Provider value={theme}>{children}</ThemeContext.Provider>
  );
}

export function useTheme() {
  return useContext(ThemeContext);
}
