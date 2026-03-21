import { createContext, useContext } from "react";
import useComparisonHistory from "../hooks/useComparisonHistory";

const FinanceContext = createContext(null);

export function FinanceProvider({ children }) {
  const comparisonHistory = useComparisonHistory();

  return (
    <FinanceContext.Provider value={{ comparisonHistory }}>
      {children}
    </FinanceContext.Provider>
  );
}

export function useFinanceContext() {
  const value = useContext(FinanceContext);
  if (!value) {
    throw new Error("useFinanceContext must be used within FinanceProvider");
  }
  return value;
}
