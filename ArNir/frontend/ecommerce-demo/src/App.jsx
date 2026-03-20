import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import EcommerceLayout from "./components/EcommerceLayout";
import ProductAdvisorPage from "./components/ProductAdvisorPage";
import CatalogUploadPage from "./components/CatalogUploadPage";

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
            border: "1px solid #ffedd5",
            borderRadius: "8px",
          },
        }}
      />
      <EcommerceLayout>
        <Routes>
          <Route path="/" element={<ProductAdvisorPage />} />
          <Route path="/upload" element={<CatalogUploadPage />} />
        </Routes>
      </EcommerceLayout>
    </BrowserRouter>
  );
}
