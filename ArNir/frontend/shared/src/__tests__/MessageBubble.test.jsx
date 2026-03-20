import { render, screen } from "@testing-library/react";
import MessageBubble from "../components/MessageBubble";

describe("MessageBubble", () => {
  it("renders user message", () => {
    render(<MessageBubble role="user" text="Hello world" />);
    expect(screen.getByText("Hello world")).toBeInTheDocument();
  });

  it("renders assistant message with markdown", () => {
    render(<MessageBubble role="assistant" text="**bold** text" />);
    expect(screen.getByText("bold")).toBeInTheDocument();
  });

  it("renders error style", () => {
    render(<MessageBubble role="assistant" text="Error occurred" isError />);
    expect(screen.getByText("Error occurred")).toBeInTheDocument();
  });
});
