import { useEffect, useMemo, useState } from "react";
import { ChevronLeft, ChevronRight, FileSearch } from "lucide-react";

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function highlightText(text, terms) {
  if (!text || terms.length === 0) {
    return [text];
  }

  const regex = new RegExp(`(${terms.map(escapeRegExp).join("|")})`, "gi");
  return text.split(regex);
}

export default function PdfViewer({ document, activeChunkText = "", highlightTerms = [] }) {
  const pages = document?.chunks?.length ? document.chunks : [];
  const initialPage = useMemo(() => {
    if (!activeChunkText || pages.length === 0) {
      return 0;
    }

    const matchIndex = pages.findIndex((chunk) => chunk.text?.includes(activeChunkText));
    return matchIndex >= 0 ? matchIndex : 0;
  }, [activeChunkText, pages]);

  const [pageIndex, setPageIndex] = useState(initialPage);

  useEffect(() => {
    setPageIndex(initialPage);
  }, [initialPage, document?.id]);

  if (!document) {
    return (
      <div className="flex h-full items-center justify-center rounded-xl border border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/40 p-6 text-center">
        <div>
          <FileSearch className="mx-auto mb-3 text-gray-400" size={28} />
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Select a source document to inspect retrieved context.
          </p>
        </div>
      </div>
    );
  }

  const page = pages[pageIndex];
  const terms = Array.from(
    new Set([...(highlightTerms || []), ...(activeChunkText ? [activeChunkText] : [])].filter(Boolean))
  );
  const fragments = highlightText(page?.text || "", terms);

  return (
    <div className="flex h-full flex-col rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900">
      <div className="flex items-center justify-between gap-3 border-b border-gray-200 dark:border-gray-700 px-4 py-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-gray-900 dark:text-gray-100">
            {document.name}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400">
            Chunk page {pageIndex + 1} of {pages.length || 1}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setPageIndex((current) => Math.max(0, current - 1))}
            disabled={pageIndex === 0}
            className="rounded-lg border border-gray-200 px-2 py-1 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800"
          >
            <ChevronLeft size={16} />
          </button>
          <button
            type="button"
            onClick={() => setPageIndex((current) => Math.min(pages.length - 1, current + 1))}
            disabled={pageIndex >= pages.length - 1}
            className="rounded-lg border border-gray-200 px-2 py-1 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      </div>
      <div className="flex-1 overflow-y-auto px-4 py-4">
        <p className="whitespace-pre-wrap text-sm leading-7 text-gray-700 dark:text-gray-200">
          {fragments.map((fragment, index) => {
            const isHighlighted = terms.some(
              (term) => term.toLowerCase() === fragment?.toLowerCase()
            );
            return isHighlighted ? (
              <mark
                key={`${fragment}-${index}`}
                className="rounded bg-amber-200/70 px-1 text-gray-900"
              >
                {fragment}
              </mark>
            ) : (
              <span key={`${fragment}-${index}`}>{fragment}</span>
            );
          })}
        </p>
      </div>
    </div>
  );
}
