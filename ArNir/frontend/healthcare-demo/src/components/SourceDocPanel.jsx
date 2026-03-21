import { useEffect, useMemo, useState } from "react";
import { getDocumentById } from "@arnir/shared";
import { BookOpen, FileText } from "lucide-react";
import PdfViewer from "./PdfViewer";
import { getHighlightTerms } from "../utils/medicalTerms";

export default function SourceDocPanel({ chunks = [] }) {
  const [activeDocumentId, setActiveDocumentId] = useState(null);
  const [documentDetail, setDocumentDetail] = useState(null);
  const [loading, setLoading] = useState(false);

  const groupedSources = useMemo(() => {
    const sourceMap = new Map();

    chunks.forEach((chunk) => {
      if (!sourceMap.has(chunk.documentId)) {
        sourceMap.set(chunk.documentId, {
          documentId: chunk.documentId,
          documentTitle: chunk.documentTitle,
          chunks: [],
        });
      }

      sourceMap.get(chunk.documentId).chunks.push(chunk);
    });

    return Array.from(sourceMap.values());
  }, [chunks]);

  useEffect(() => {
    if (groupedSources.length > 0 && !groupedSources.some((source) => source.documentId === activeDocumentId)) {
      setActiveDocumentId(groupedSources[0].documentId);
    }
    if (groupedSources.length === 0) {
      setActiveDocumentId(null);
      setDocumentDetail(null);
    }
  }, [groupedSources, activeDocumentId]);

  useEffect(() => {
    if (!activeDocumentId) {
      return;
    }

    let cancelled = false;

    const loadDocument = async () => {
      setLoading(true);
      try {
        const response = await getDocumentById(activeDocumentId);
        if (!cancelled) {
          setDocumentDetail(response.data);
        }
      } catch {
        if (!cancelled) {
          setDocumentDetail(null);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadDocument();

    return () => {
      cancelled = true;
    };
  }, [activeDocumentId]);

  const activeSource = groupedSources.find((source) => source.documentId === activeDocumentId);
  const activeChunkText = activeSource?.chunks?.[0]?.chunkText || "";
  const highlightTerms = getHighlightTerms(activeChunkText);

  return (
    <div className="flex h-full flex-col gap-4">
      <div className="flex items-center gap-2">
        <BookOpen className="text-primary-600 dark:text-primary-400" size={20} />
        <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-100">
          Source Documents
        </h3>
      </div>

      {groupedSources.length === 0 ? (
        <div className="rounded-xl border border-dashed border-gray-300 dark:border-gray-700 p-6 text-center">
          <FileText className="mx-auto mb-3 text-gray-300 dark:text-gray-600" size={36} />
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Source citations will appear here after a medical answer is generated.
          </p>
        </div>
      ) : (
        <>
          <div className="grid gap-2">
            {groupedSources.map((source) => {
              const isActive = source.documentId === activeDocumentId;
              return (
                <button
                  key={source.documentId}
                  type="button"
                  onClick={() => setActiveDocumentId(source.documentId)}
                  className={`rounded-xl border px-3 py-3 text-left transition ${
                    isActive
                      ? "border-primary-300 bg-primary-50 dark:border-primary-700 dark:bg-primary-900/20"
                      : "border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800"
                  }`}
                >
                  <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                    {source.documentTitle}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    {source.chunks.length} retrieved chunk{source.chunks.length !== 1 ? "s" : ""}
                  </p>
                </button>
              );
            })}
          </div>

          <div className="min-h-0 flex-1">
            {loading ? (
              <div className="flex h-full items-center justify-center rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-500 dark:text-gray-400">
                Loading document view...
              </div>
            ) : (
              <PdfViewer
                document={documentDetail}
                activeChunkText={activeChunkText}
                highlightTerms={highlightTerms}
              />
            )}
          </div>
        </>
      )}
    </div>
  );
}
