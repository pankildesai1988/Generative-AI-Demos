import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { AnalyticsProvider, ErrorBoundary } from "@arnir/shared";
import HealthcareLayout from "./components/HealthcareLayout";
import MedicalChatPage from "./components/MedicalChatPage";
import MedicalUploadPage from "./components/MedicalUploadPage";

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
              border: "1px solid #ccfbf1",
              borderRadius: "8px",
            },
          }}
        />
        <ErrorBoundary>
          <HealthcareLayout>
            <Routes>
              <Route path="/" element={<MedicalChatPage />} />
              <Route path="/upload" element={<MedicalUploadPage />} />
            </Routes>
          </HealthcareLayout>
        </ErrorBoundary>
      </AnalyticsProvider>
    </BrowserRouter>
  );
}
