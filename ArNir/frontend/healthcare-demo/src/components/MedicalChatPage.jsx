import { useMemo, useState } from "react";
import { useChat, ChatWindow } from "@arnir/shared";
import DocumentSelector from "./DocumentSelector";
import ExportButton from "./ExportButton";
import HighlightedMessage from "./HighlightedMessage";
import SourceDocPanel from "./SourceDocPanel";
import useDocumentList from "../hooks/useDocumentList";

export default function MedicalChatPage() {
  const { documents, loading: documentsLoading, error: documentsError, refreshDocuments } =
    useDocumentList();
  const [selectedDocumentIds, setSelectedDocumentIds] = useState([]);

  const chat = useChat({
    provider: "OpenAI",
    model: "gpt-4o-mini",
    promptStyle: "rag",
    topK: 5,
  });

  const handleToggleDocument = (documentId) => {
    setSelectedDocumentIds((current) =>
      current.includes(documentId)
        ? current.filter((id) => id !== documentId)
        : [...current, documentId]
    );
  };

  const handleSelectAllDocuments = () => {
    setSelectedDocumentIds(documents.map((document) => document.id));
  };

  const handleClearSelection = () => {
    setSelectedDocumentIds([]);
  };

  const handleSendMessage = (query) =>
    chat.sendMessage(query, {
      documentIds: selectedDocumentIds,
    });

  const selectedDocuments = useMemo(
    () =>
      documents
        .filter((document) => selectedDocumentIds.includes(document.id))
        .map((document) => document.name),
    [documents, selectedDocumentIds]
  );

  return (
    <div className="flex h-full flex-col gap-4 lg:flex-row">
      <div className="w-full p-4 lg:w-80 lg:flex-shrink-0 lg:pr-0">
        <DocumentSelector
          documents={documents}
          selectedIds={selectedDocumentIds}
          loading={documentsLoading}
          error={documentsError}
          onToggle={handleToggleDocument}
          onSelectAll={handleSelectAllDocuments}
          onClear={handleClearSelection}
          onRefresh={refreshDocuments}
        />
      </div>

      <div className="flex min-h-0 flex-1 flex-col p-4 pt-0 lg:pt-4">
        <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Medical Knowledge Assistant
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              Ask grounded questions across one or more uploaded medical documents.
            </p>
          </div>
          <ExportButton messages={chat.messages} selectedDocuments={selectedDocuments} />
        </div>

        <ChatWindow
          messages={chat.messages}
          onSend={handleSendMessage}
          loading={chat.loading}
          lastHistoryId={chat.lastHistoryId}
          onClear={chat.clearChat}
          title="Medical Knowledge Assistant"
          placeholder="Ask about symptoms, treatments, drug interactions..."
          renderMessage={(message, index) => (
            <HighlightedMessage
              key={`${message.role}-${index}-${message.text.slice(0, 20)}`}
              message={message}
            />
          )}
        />
      </div>

      <div className="min-h-0 w-full border-t bg-white p-4 dark:border-gray-700 dark:bg-gray-900 lg:block lg:w-96 lg:border-l lg:border-t-0">
        <SourceDocPanel chunks={chat.chunks} />
      </div>
    </div>
  );
}
