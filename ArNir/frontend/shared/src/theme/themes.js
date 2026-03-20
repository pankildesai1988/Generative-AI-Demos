/**
 * Runtime theme definitions for chart colors and metadata.
 * TailwindCSS handles build-time colors via per-demo tailwind.config.js.
 * These are for runtime-only values (Recharts, dynamic styles, etc.)
 */
export const themes = {
  healthcare: {
    name: "Healthcare Knowledge Assistant",
    icon: "Heart",
    chartPrimary: "#14b8a6",
    chartSecondary: "#10b981",
    chartAccent: "#0d9488",
    gradientFrom: "#f0fdfa",
    gradientTo: "#ccfbf1",
    dark: {
      chartPrimary: "#2dd4bf",
      chartSecondary: "#34d399",
      chartAccent: "#14b8a6",
      gradientFrom: "#042f2e",
      gradientTo: "#134e4a",
    },
  },
  ecommerce: {
    name: "Ecommerce Product Advisor",
    icon: "ShoppingCart",
    chartPrimary: "#f97316",
    chartSecondary: "#f59e0b",
    chartAccent: "#ea580c",
    gradientFrom: "#fff7ed",
    gradientTo: "#fed7aa",
    dark: {
      chartPrimary: "#fb923c",
      chartSecondary: "#fbbf24",
      chartAccent: "#f97316",
      gradientFrom: "#431407",
      gradientTo: "#7c2d12",
    },
  },
  finance: {
    name: "Financial Document Analyzer",
    icon: "BarChart3",
    chartPrimary: "#1e3a5f",
    chartSecondary: "#fbbf24",
    chartAccent: "#334155",
    gradientFrom: "#f8fafc",
    gradientTo: "#e2e8f0",
    dark: {
      chartPrimary: "#94a3b8",
      chartSecondary: "#fbbf24",
      chartAccent: "#64748b",
      gradientFrom: "#0f172a",
      gradientTo: "#1e293b",
    },
  },
};
