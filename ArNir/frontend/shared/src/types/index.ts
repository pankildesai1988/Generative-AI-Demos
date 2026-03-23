import type { AxiosInstance } from "axios";
import type { ReactNode, RefObject } from "react";

// ── Chat & Messages ──────────────────────────────────────────────

export interface RetrievedChunk {
  chunkText?: string;
  text?: string;
  content?: string;
  documentTitle?: string;
  rank?: number;
  retrievalType?: string;
}

export interface Message {
  role: "user" | "assistant";
  text: string;
  isError?: boolean;
  isStreaming?: boolean;
  id?: string;
  chunks?: RetrievedChunk[];
}

export interface ChatConfig {
  provider?: string;
  model?: string;
  promptStyle?: string;
  topK?: number;
  useHybrid?: boolean;
  documentIds?: string[];
}

export interface ChatHookReturn {
  messages: Message[];
  sendMessage: (query: string, options?: { documentIds?: string[] }) => Promise<void>;
  loading: boolean;
  lastHistoryId: string | null;
  chunks: RetrievedChunk[];
  error: string | null;
  clearChat: () => void;
}

// ── RAG ──────────────────────────────────────────────────────────

export interface RagPayload {
  query: string;
  topK?: number;
  useHybrid?: boolean;
  promptStyle?: string;
  saveAsNew?: boolean;
  provider?: string;
  model?: string;
  documentIds?: string[];
}

export interface RagQueryResult {
  ragAnswer: string;
  retrievedChunks: RetrievedChunk[];
  historyId: string;
}

// ── Streaming ────────────────────────────────────────────────────

export interface StreamHandlers {
  onToken?: (payload: { token: string }) => void;
  onMetadata?: (payload: { historyId: string; chunks: RetrievedChunk[] }) => void;
  onComplete?: (payload: Record<string, unknown>) => void;
  onError?: (error: Error) => void;
  signal?: AbortSignal;
}

export interface SseEvent {
  event: string;
  data: unknown;
}

// ── Analytics ────────────────────────────────────────────────────

export interface AnalyticsEvent {
  category: string;
  action: string;
  label: string | null;
  metadata: Record<string, unknown>;
  timestamp: string;
}

export interface AnalyticsBackend {
  track: (event: AnalyticsEvent) => void;
}

export interface AnalyticsContextValue {
  trackEvent: (
    category: string,
    action: string,
    label?: string,
    metadata?: Record<string, unknown>,
  ) => AnalyticsEvent;
}

// ── Theme ────────────────────────────────────────────────────────

export interface ThemeDarkColors {
  chartPrimary: string;
  chartSecondary: string;
  chartAccent: string;
  gradientFrom: string;
  gradientTo: string;
}

export interface ThemeConfig {
  name: string;
  icon: string;
  chartPrimary: string;
  chartSecondary: string;
  chartAccent: string;
  gradientFrom: string;
  gradientTo: string;
  dark: ThemeDarkColors;
}

export interface ThemeContextValue extends ThemeConfig {
  mode: "light" | "dark";
  toggleMode: () => void;
}

export type DemoType = "healthcare" | "ecommerce" | "finance";

export type Themes = Record<DemoType, ThemeConfig>;

// ── File Upload ──────────────────────────────────────────────────

export interface FileUploadResult {
  message: string;
  documentId?: string;
}

export interface FileUploadHookReturn {
  uploadFile: (file: File, uploadedBy?: string) => Promise<void>;
  uploading: boolean;
  error: string | null;
  result: FileUploadResult | null;
  reset: () => void;
}

// ── Components Props ─────────────────────────────────────────────

export interface ChatWindowProps {
  messages: Message[];
  onSend: (query: string) => void;
  loading: boolean;
  lastHistoryId?: string | null;
  onClear: () => void;
  placeholder?: string;
  title?: string;
  renderMessage?: (message: Message, index: number) => ReactNode;
}

export interface MessageBubbleProps {
  role: string;
  text: string;
  isError?: boolean;
}

export interface ErrorBannerProps {
  message?: string;
  onRetry?: () => void;
}

export interface FeedbackModalProps {
  historyId?: string;
  onClose: () => void;
}

export interface FileUploadProps {
  onUpload: (file: File) => void;
  uploading: boolean;
  error?: string | null;
  result?: FileUploadResult | null;
  onReset?: () => void;
  acceptedTypes?: string;
  guidance?: string;
}

export interface SourceViewerProps {
  chunks?: RetrievedChunk[];
  title?: string;
}

export interface SkeletonProps {
  variant?: "text" | "circle" | "card" | "chat-bubble";
  className?: string;
  count?: number;
}

export interface CardSkeletonProps {
  lines?: number;
}

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "accent" | "ghost";
  children: ReactNode;
}

export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {}

// ── API Client ───────────────────────────────────────────────────

export type ApiClient = AxiosInstance;

// ── Runtime Config ───────────────────────────────────────────────

declare global {
  interface Window {
    __RUNTIME_CONFIG__?: {
      API_URL?: string;
    };
  }
}
