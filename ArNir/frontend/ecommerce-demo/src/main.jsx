import React from "react";
import ReactDOM from "react-dom/client";
import { ThemeProvider } from "@arnir/shared";
import App from "./App";
import "./index.css";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <ThemeProvider demoType="ecommerce">
      <App />
    </ThemeProvider>
  </React.StrictMode>
);
