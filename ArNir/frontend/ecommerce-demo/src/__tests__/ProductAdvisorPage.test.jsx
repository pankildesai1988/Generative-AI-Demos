import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CommerceProvider } from "../context/CommerceContext";

vi.mock("@arnir/shared", () => ({
  useChat: () => ({
    messages: [], sendMessage: vi.fn(), loading: false,
    lastHistoryId: null,
    chunks: [
      { chunkText: "ProBook Ultra 15\nPrice: $599\nCategory: Student / Everyday\nBest for: Great for students", documentTitle: "Laptops", documentId: 10, rank: 1 },
      { chunkText: "GameStorm X17\nPrice: $1,299\nCategory: Gaming\nBest for: For gamers", documentTitle: "Laptops", documentId: 11, rank: 2 },
    ],
    error: null, clearChat: vi.fn(),
  }),
  ChatWindow: ({ title, placeholder }) => (
    <div data-testid="chat-window"><span>{title}</span><span>{placeholder}</span></div>
  ),
}));

import ProductAdvisorPage from "../components/ProductAdvisorPage";

describe("ProductAdvisorPage", () => {
  const renderPage = () =>
    render(
      <CommerceProvider>
        <ProductAdvisorPage />
      </CommerceProvider>
    );

  it("renders chat window with product advisor title", () => {
    renderPage();
    expect(screen.getByText("Product Advisor")).toBeInTheDocument();
  });

  it("renders recommendations panel", () => {
    renderPage();
    expect(screen.getByText("Recommendations")).toBeInTheDocument();
  });

  it("shows product count when chunks available", () => {
    renderPage();
    expect(screen.getByText(/2 products/)).toBeInTheDocument();
  });

  it("renders product cards from chunks", () => {
    renderPage();
    expect(screen.getByText(/ProBook Ultra 15/)).toBeInTheDocument();
    expect(screen.getByText("$599")).toBeInTheDocument();
  });

  it("renders budget and facet controls", () => {
    renderPage();
    expect(screen.getByText("Budget Range")).toBeInTheDocument();
    expect(screen.getByText("Facets")).toBeInTheDocument();
    expect(screen.getAllByText("Student / Everyday").length).toBeGreaterThan(0);
  });
});
