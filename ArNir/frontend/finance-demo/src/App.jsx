import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { AnalyticsProvider, ErrorBoundary } from "@arnir/shared";
import FinanceLayout from "./components/FinanceLayout";
import FinanceChatPage from "./components/FinanceChatPage";
import FinancialUploadPage from "./components/FinancialUploadPage";
import FinanceComparePage from "./components/FinanceComparePage";
import { FinanceProvider } from "./components/FinanceContext";

export default function App() {
  return (
    <BrowserRouter>
      <AnalyticsProvider>
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: {
              background: "#fff",
              color: "#333",
              border: "1px solid #e2e8f0",
              borderRadius: "8px",
            },
          }}
        />
        <ErrorBoundary>
          <FinanceProvider>
            <FinanceLayout>
              <Routes>
                <Route path="/" element={<FinanceChatPage />} />
                <Route path="/upload" element={<FinancialUploadPage />} />
                <Route path="/compare" element={<FinanceComparePage />} />
              </Routes>
            </FinanceLayout>
          </FinanceProvider>
        </ErrorBoundary>
      </AnalyticsProvider>
    </BrowserRouter>
  );
}
