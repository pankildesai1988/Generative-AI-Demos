import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("@arnir/shared", () => ({
  useChat: () => ({
    messages: [], sendMessage: vi.fn(), loading: false,
    lastHistoryId: null,
    chunks: [
      { chunkText: "ProBook Ultra 15\nPrice: $599\nGreat for students", documentTitle: "Laptops", rank: 1 },
      { chunkText: "GameStorm X17\nPrice: $1,299\nFor gamers", documentTitle: "Laptops", rank: 2 },
    ],
    error: null, clearChat: vi.fn(),
  }),
  ChatWindow: ({ title, placeholder }) => (
    <div data-testid="chat-window"><span>{title}</span><span>{placeholder}</span></div>
  ),
}));

import ProductAdvisorPage from "../components/ProductAdvisorPage";

describe("ProductAdvisorPage", () => {
  it("renders chat window with product advisor title", () => {
    render(<ProductAdvisorPage />);
    expect(screen.getByText("Product Advisor")).toBeInTheDocument();
  });

  it("renders recommendations panel", () => {
    render(<ProductAdvisorPage />);
    expect(screen.getByText("Recommendations")).toBeInTheDocument();
  });

  it("shows product count when chunks available", () => {
    render(<ProductAdvisorPage />);
    expect(screen.getByText(/2 products/)).toBeInTheDocument();
  });

  it("renders product cards from chunks", () => {
    render(<ProductAdvisorPage />);
    expect(screen.getByText(/ProBook Ultra 15/)).toBeInTheDocument();
    expect(screen.getByText("$599")).toBeInTheDocument();
  });
});
