import { useEffect, useMemo, useState } from "react";
import { getDocumentById } from "@arnir/shared";
import { BookOpen, FileText } from "lucide-react";
import PdfViewer from "./PdfViewer";
import { getHighlightTerms } from "../utils/medicalTerms";

export default function SourceDocPanel({ chunks = [] }) {
  const [activeKey, setActiveKey] = useState(null);
  const [documentDetail, setDocumentDetail] = useState(null);
  const [loading, setLoading] = useState(false);

  // Group by documentId when present, fall back to documentTitle, then "unknown"
  const groupedSources = useMemo(() => {
    const sourceMap = new Map();

    chunks.forEach((chunk) => {
      const key =
        chunk.documentId != null
          ? String(chunk.documentId)
          : chunk.documentTitle || "unknown";

      if (!sourceMap.has(key)) {
        sourceMap.set(key, {
          key,
          documentId: chunk.documentId,
          documentTitle: chunk.documentTitle || "Source Document",
          chunks: [],
        });
      }

      sourceMap.get(key).chunks.push(chunk);
    });

    return Array.from(sourceMap.values());
  }, [chunks]);

  // Auto-select first group when sources change
  useEffect(() => {
    if (groupedSources.length > 0 && !groupedSources.some((s) => s.key === activeKey)) {
      setActiveKey(groupedSources[0].key);
      setDocumentDetail(null);
    }
    if (groupedSources.length === 0) {
      setActiveKey(null);
      setDocumentDetail(null);
    }
  }, [groupedSources, activeKey]);

  // Fetch full document detail when activeKey changes (only if we have a numeric documentId)
  useEffect(() => {
    const activeSource = groupedSources.find((s) => s.key === activeKey);
    if (!activeSource?.documentId) {
      setDocumentDetail(null);
      return;
    }

    let cancelled = false;

    const loadDocument = async () => {
      setLoading(true);
      try {
        const response = await getDocumentById(activeSource.documentId);
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
  }, [activeKey, groupedSources]);

  const activeSource = groupedSources.find((s) => s.key === activeKey);
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
              const isActive = source.key === activeKey;
              return (
                <button
                  key={source.key}
                  type="button"
                  onClick={() => setActiveKey(source.key)}
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
            ) : documentDetail ? (
              <PdfViewer
                document={documentDetail}
                activeChunkText={activeChunkText}
                highlightTerms={highlightTerms}
              />
            ) : (
              // Fallback: render chunk text directly when document detail is unavailable
              <div className="flex h-full flex-col gap-3 overflow-y-auto rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
                {activeSource?.chunks.map((chunk, idx) => (
                  <div
                    key={idx}
                    className="rounded-lg border border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-3"
                  >
                    <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-primary-600 dark:text-primary-400">
                      Chunk {idx + 1}
                      {chunk.retrievalType ? ` · ${chunk.retrievalType}` : ""}
                    </p>
                    <p className="whitespace-pre-wrap text-sm text-gray-800 dark:text-gray-200 leading-relaxed">
                      {chunk.chunkText || chunk.text || chunk.content || "No content available."}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
