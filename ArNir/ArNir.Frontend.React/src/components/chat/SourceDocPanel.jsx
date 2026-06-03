import { useEffect, useMemo, useState } from "react";
import { BookOpen, FileText } from "lucide-react";
import { getDocument } from "../../api/documents";
import { getRuntimeApiUrl } from "../../config/runtime";
import { getHighlightLabels } from "../../utils/highlightTerms";
import PdfJsViewer from "./PdfJsViewer";
import PdfViewer from "./PdfViewer";
import SourceViewer from "../shared/SourceViewer";

const API_BASE = getRuntimeApiUrl();

export default function SourceDocPanel({ chunks = [] }) {
  const [activeKey, setActiveKey] = useState(null);
  const [documentDetail, setDocumentDetail] = useState(null);
  const [loading, setLoading] = useState(false);
  const [pdfHidden, setPdfHidden] = useState(false);

  const groupedSources = useMemo(() => {
    const map = new Map();
    chunks.forEach((chunk) => {
      const key =
        chunk.documentId != null
          ? String(chunk.documentId)
          : chunk.documentTitle || "unknown";
      if (!map.has(key)) {
        map.set(key, {
          key,
          documentId: chunk.documentId,
          documentTitle: chunk.documentTitle || chunk.fileName || "Source Document",
          chunks: [],
        });
      }
      map.get(key).chunks.push(chunk);
    });
    return Array.from(map.values());
  }, [chunks]);

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

  useEffect(() => {
    setPdfHidden(false);
  }, [activeKey]);

  useEffect(() => {
    const active = groupedSources.find((s) => s.key === activeKey);
    if (!active?.documentId) {
      setDocumentDetail(null);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const res = await getDocument(active.documentId);
        if (!cancelled) setDocumentDetail(res.data);
      } catch {
        if (!cancelled) setDocumentDetail(null);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [activeKey, groupedSources]);

  const activeSource = groupedSources.find((s) => s.key === activeKey);
  const activeChunk = activeSource?.chunks?.[0];
  const activeChunkText = activeChunk?.chunkText || activeChunk?.text || "";
  const highlightTerms = getHighlightLabels(activeChunkText);
  const hasPdfPosition =
    activeChunk?.pageNumber != null && activeSource?.documentId != null;

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
            Source citations will appear here after the assistant answers.
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
                Loading document…
              </div>
            ) : hasPdfPosition && !pdfHidden ? (
              <PdfJsViewer
                documentId={Number(activeSource.documentId)}
                pageNumber={activeChunk.pageNumber}
                bbox={
                  activeChunk.bboxX1 != null
                    ? {
                        x1: activeChunk.bboxX1,
                        y1: activeChunk.bboxY1,
                        x2: activeChunk.bboxX2,
                        y2: activeChunk.bboxY2,
                      }
                    : null
                }
                chunkType={activeChunk.chunkType}
                apiBaseUrl={API_BASE}
                onClose={() => setPdfHidden(true)}
              />
            ) : hasPdfPosition && pdfHidden ? (
              <div className="flex h-full flex-col gap-3 overflow-y-auto rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
                <button
                  type="button"
                  onClick={() => setPdfHidden(false)}
                  className="self-start rounded-lg bg-primary-50 px-3 py-1.5 text-xs font-medium text-primary-700 hover:bg-primary-100 dark:bg-primary-900/30 dark:text-primary-300"
                >
                  Show PDF preview
                </button>
                <SourceViewer chunks={activeSource?.chunks ?? []} title="Chunks" />
              </div>
            ) : documentDetail ? (
              <PdfViewer
                document={documentDetail}
                activeChunkText={activeChunkText}
                highlightTerms={highlightTerms}
              />
            ) : (
              <div className="flex h-full flex-col gap-3 overflow-y-auto rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
                <SourceViewer chunks={activeSource?.chunks ?? []} title="Chunks" />
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
