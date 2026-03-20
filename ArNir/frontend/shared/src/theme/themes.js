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
  },
  ecommerce: {
    name: "Ecommerce Product Advisor",
    icon: "ShoppingCart",
    chartPrimary: "#f97316",
    chartSecondary: "#f59e0b",
    chartAccent: "#ea580c",
    gradientFrom: "#fff7ed",
    gradientTo: "#fed7aa",
  },
  finance: {
    name: "Financial Document Analyzer",
    icon: "BarChart3",
    chartPrimary: "#1e3a5f",
    chartSecondary: "#fbbf24",
    chartAccent: "#334155",
    gradientFrom: "#f8fafc",
    gradientTo: "#e2e8f0",
  },
};
