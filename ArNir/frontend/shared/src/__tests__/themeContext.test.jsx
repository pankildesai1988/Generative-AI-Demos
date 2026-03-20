import { render, screen, fireEvent } from "@testing-library/react";
import { ThemeProvider, useTheme } from "../theme/themeContext";

function ThemeConsumer() {
  const { name, mode, toggleMode } = useTheme();
  return (
    <div>
      <span data-testid="name">{name}</span>
      <span data-testid="mode">{mode}</span>
      <button onClick={toggleMode}>Toggle</button>
    </div>
  );
}

describe("ThemeProvider", () => {
  beforeEach(() => localStorage.clear());

  it("provides healthcare theme by default", () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>);
    expect(screen.getByTestId("name")).toHaveTextContent("Healthcare Knowledge Assistant");
  });

  it("provides ecommerce theme", () => {
    render(<ThemeProvider demoType="ecommerce"><ThemeConsumer /></ThemeProvider>);
    expect(screen.getByTestId("name")).toHaveTextContent("Ecommerce Product Advisor");
  });

  it("defaults to light mode", () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>);
    expect(screen.getByTestId("mode")).toHaveTextContent("light");
  });

  it("toggles to dark mode", () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>);
    fireEvent.click(screen.getByText("Toggle"));
    expect(screen.getByTestId("mode")).toHaveTextContent("dark");
  });

  it("persists mode to localStorage", () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>);
    fireEvent.click(screen.getByText("Toggle"));
    expect(localStorage.getItem("arnir-theme-mode")).toBe("dark");
  });
});
