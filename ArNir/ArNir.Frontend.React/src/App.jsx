import { BrowserRouter, Routes, Route } from "react-router-dom";
import Chat from "./components/chat/Chat";
import AnalyticsPage from "./pages/AnalyticsPage";
import InsightsPage from "./pages/InsightsPage";
import IntelligencePage from "./pages/IntelligencePage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Chat />} />
        <Route path="/analytics" element={<AnalyticsPage />} />
        <Route path="/insight" element={<InsightsPage />} />
        <Route path="/intelligence" element={<IntelligencePage />} />
      </Routes>
    </BrowserRouter>
  );
}
