import { createContext, useContext, useEffect, useMemo } from "react";
import { useLocation } from "react-router-dom";
import { createConsoleBackend, setAnalyticsBackend, trackEvent } from "./tracker";

const AnalyticsContext = createContext({
  trackEvent,
});

export function AnalyticsProvider({ children, backend }) {
  const location = useLocation();
  const resolvedBackend = useMemo(
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

  const value = useMemo(
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

export function useAnalytics() {
  return useContext(AnalyticsContext);
}
