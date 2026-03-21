// API modules
export { default as api } from "./api/client";
export { runRag, testRetrieval } from "./api/rag";
export { sendChatQuery, getSessionContext } from "./api/chat";
export { submitFeedback, getFeedbacks, getAverageRating } from "./api/feedback";
export { ingestDocument, getDocuments, getDocumentById } from "./api/documents";
export { evaluate, getEvaluationHistory, getEvaluationStats } from "./api/evaluation";

// Hooks
export { default as useChat } from "./hooks/useChat";
export { default as useFileUpload } from "./hooks/useFileUpload";
export { default as useFocusTrap } from "./hooks/useFocusTrap";
export { default as useKeyboardNav } from "./hooks/useKeyboardNav";

// Components
export { default as ChatWindow } from "./components/ChatWindow";
export { default as FileUpload } from "./components/FileUpload";
export { default as SourceViewer } from "./components/SourceViewer";
export { default as FeedbackModal } from "./components/FeedbackModal";
export { default as MessageBubble } from "./components/MessageBubble";
export { default as TypingIndicator } from "./components/TypingIndicator";
export { default as Loader } from "./components/Loader";
export { default as ErrorBanner } from "./components/ErrorBanner";
export { default as ErrorBoundary } from "./components/ErrorBoundary";
export { default as Skeleton } from "./components/Skeleton";
export { default as ChatSkeleton } from "./components/ChatSkeleton";
export { default as CardSkeleton } from "./components/CardSkeleton";

// UI primitives
export { Button } from "./ui/button";
export { Card, CardHeader, CardContent } from "./ui/card";
export { Input } from "./ui/input";

// Theme
export { themes } from "./theme/themes";
export { ThemeProvider, useTheme } from "./theme/themeContext";
