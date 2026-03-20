import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import FinanceLayout from "./components/FinanceLayout";
import FinanceChatPage from "./components/FinanceChatPage";
import FinancialUploadPage from "./components/FinancialUploadPage";

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
            border: "1px solid #e2e8f0",
            borderRadius: "8px",
          },
        }}
      />
      <FinanceLayout>
        <Routes>
          <Route path="/" element={<FinanceChatPage />} />
          <Route path="/upload" element={<FinancialUploadPage />} />
        </Routes>
      </FinanceLayout>
    </BrowserRouter>
  );
}
