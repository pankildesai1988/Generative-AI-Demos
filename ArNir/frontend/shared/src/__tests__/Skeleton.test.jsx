import { render } from "@testing-library/react";
import Skeleton from "../components/Skeleton";
import ChatSkeleton from "../components/ChatSkeleton";
import CardSkeleton from "../components/CardSkeleton";

describe("Skeleton", () => {
  it("renders with default variant", () => {
    const { container } = render(<Skeleton />);
    expect(container.querySelector(".animate-pulse")).toBeInTheDocument();
  });

  it("renders multiple items", () => {
    const { container } = render(<Skeleton count={3} />);
    expect(container.querySelectorAll(".animate-pulse")).toHaveLength(3);
  });
});

describe("ChatSkeleton", () => {
  it("renders chat layout skeleton", () => {
    const { container } = render(<ChatSkeleton />);
    expect(container.querySelector(".animate-pulse")).toBeInTheDocument();
  });
});

describe("CardSkeleton", () => {
  it("renders card skeleton with lines", () => {
    const { container } = render(<CardSkeleton lines={4} />);
    expect(container.querySelector(".animate-pulse")).toBeInTheDocument();
  });
});
