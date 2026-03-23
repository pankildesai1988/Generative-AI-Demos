import { createContext, useContext, useEffect, useMemo } from "react";
import type { AnalyticsBackend, AnalyticsContextValue } from "../types";
import { useLocation } from "react-router-dom";
import { createConsoleBackend, setAnalyticsBackend, trackEvent } from "./tracker";

const AnalyticsContext = createContext<AnalyticsContextValue>({
  trackEvent,
});

interface AnalyticsProviderProps {
  children: React.ReactNode;
  backend?: AnalyticsBackend;
}

export function AnalyticsProvider({ children, backend }: AnalyticsProviderProps): React.ReactElement {
  const location = useLocation();
  const resolvedBackend = useMemo<AnalyticsBackend>(
    () => backend ?? createConsoleBackend(),
    [backend]
  );

  useEffect(() => {
    const previousBackend = setAnalyticsBackend(resolvedBackend);
    return () => {
      setAnalyticsBackend(previousBackend);
    };
  }, [resolvedBackend]);

  useEffect(() => {
    trackEvent("navigation", "page_view", location.pathname, {
      search: location.search || "",
    });
  }, [location.pathname, location.search]);

  const value = useMemo<AnalyticsContextValue>(
    () => ({
      trackEvent,
    }),
    []
  );

  return (
    <AnalyticsContext.Provider value={value}>
      {children}
    </AnalyticsContext.Provider>
  );
}

export function useAnalytics(): AnalyticsContextValue {
  return useContext(AnalyticsContext);
}
