import { render, screen, fireEvent } from "@testing-library/react";
import ChatWindow from "../components/ChatWindow";

// jsdom doesn't implement scrollIntoView
Element.prototype.scrollIntoView = vi.fn();

vi.mock("../components/FeedbackModal", () => ({ default: () => null }));
vi.mock("../components/TypingIndicator", () => ({ default: () => <div data-testid="typing" /> }));

describe("ChatWindow", () => {
  const defaultProps = {
    messages: [],
    onSend: vi.fn(),
    loading: false,
    lastHistoryId: null,
    onClear: vi.fn(),
    title: "Test Assistant",
    placeholder: "Type here...",
  };

  it("renders title", () => {
    render(<ChatWindow {...defaultProps} />);
    expect(screen.getByText("Test Assistant")).toBeInTheDocument();
  });

  it("shows empty state", () => {
    render(<ChatWindow {...defaultProps} />);
    expect(screen.getByText(/Start a conversation/)).toBeInTheDocument();
  });

  it("renders messages", () => {
    const messages = [
      { role: "user", text: "Hello" },
      { role: "assistant", text: "Hi there" },
    ];
    render(<ChatWindow {...defaultProps} messages={messages} />);
    expect(screen.getByText("Hello")).toBeInTheDocument();
    expect(screen.getByText("Hi there")).toBeInTheDocument();
  });

  it("calls onSend on Enter key", () => {
    const onSend = vi.fn();
    render(<ChatWindow {...defaultProps} onSend={onSend} />);
    const input = screen.getByPlaceholderText("Type here...");
    fireEvent.change(input, { target: { value: "test query" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onSend).toHaveBeenCalledWith("test query");
  });

  it("shows typing indicator when loading", () => {
    render(<ChatWindow {...defaultProps} loading={true} />);
    expect(screen.getByTestId("typing")).toBeInTheDocument();
  });

  it("shows clear button with messages", () => {
    render(<ChatWindow {...defaultProps} messages={[{ role: "user", text: "Hi" }]} />);
    const clearBtn = screen.getByTitle("Clear chat");
    expect(clearBtn).toBeInTheDocument();
  });
});
