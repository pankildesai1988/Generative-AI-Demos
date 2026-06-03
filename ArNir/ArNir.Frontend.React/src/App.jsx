import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import AppLayout from "./components/layout/AppLayout";
import Chat from "./components/chat/Chat";
import AnalyticsPage from "./pages/AnalyticsPage";
import InsightsPage from "./pages/InsightsPage";
import IntelligencePage from "./pages/IntelligencePage";
import InsightChatPage from "./pages/InsightChatPage";
import DocumentUploadPage from "./pages/DocumentUploadPage";
import { AnalyticsProvider } from "./analytics/AnalyticsProvider";

export default function App() {
  return (
    <BrowserRouter>
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            background: "#fff",
            color: "#333",
            border: "1px solid #e5e7eb",
            borderRadius: "8px",
            boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
          },
          success: { iconTheme: { primary: "#4ade80", secondary: "#fff" } },
          error: { iconTheme: { primary: "#f87171", secondary: "#fff" } },
        }}
      />

      <AnalyticsProvider>
        <Routes>
          <Route element={<AppLayout />}>
            <Route path="/" element={<Chat />} />
            <Route path="/analytics" element={<AnalyticsPage />} />
            <Route path="/insight" element={<InsightsPage />} />
            <Route path="/intelligence" element={<IntelligencePage />} />
            <Route path="/intelligence/chat" element={<InsightChatPage />} />
            <Route path="/upload" element={<DocumentUploadPage />} />
          </Route>
        </Routes>
      </AnalyticsProvider>
    </BrowserRouter>
  );
}
