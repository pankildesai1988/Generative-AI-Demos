const DEFAULT_API_URL: string = "https://localhost:5001/api/";
const API_PLACEHOLDER: string = "__API_URL__";

export function getRuntimeApiUrl(): string {
  if (typeof window !== "undefined") {
    const runtimeUrl = window.__RUNTIME_CONFIG__?.API_URL;
    if (
      typeof runtimeUrl === "string" &&
      runtimeUrl.trim() &&
      runtimeUrl.trim() !== API_PLACEHOLDER
    ) {
      return runtimeUrl.trim();
    }
  }

  const envUrl = import.meta.env.VITE_API_BASE_URL;
  if (typeof envUrl === "string" && envUrl.trim()) {
    return envUrl.trim();
  }

  return DEFAULT_API_URL;
}

export { DEFAULT_API_URL };
