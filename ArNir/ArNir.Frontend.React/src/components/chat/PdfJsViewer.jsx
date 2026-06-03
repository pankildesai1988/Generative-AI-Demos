import { useEffect, useRef, useState } from "react";
import * as pdfjsLib from "pdfjs-dist";
import { X } from "lucide-react";

pdfjsLib.GlobalWorkerOptions.workerSrc =
  "https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js";

const SCALE = 1.5;

export default function PdfJsViewer({
  documentId,
  pageNumber,
  bbox = null,
  chunkType,
  apiBaseUrl,
  onClose,
}) {
  const canvasRef = useRef(null);
  const renderTaskRef = useRef(null);
  const [pdfDoc, setPdfDoc] = useState(null);
  const [totalPages, setTotalPages] = useState(0);
  const [currentPage, setCurrentPage] = useState(pageNumber || 1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    (async () => {
      try {
        const res = await fetch(`${apiBaseUrl}/documents/${documentId}/file`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const buffer = await res.arrayBuffer();
        if (cancelled) return;
        const doc = await pdfjsLib.getDocument({ data: buffer }).promise;
        if (cancelled) return;
        setPdfDoc(doc);
        setTotalPages(doc.numPages);
      } catch (err) {
        if (!cancelled) setError(err?.message || "Failed to load PDF");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => { cancelled = true; };
  }, [documentId, apiBaseUrl]);

  useEffect(() => {
    setCurrentPage(pageNumber || 1);
  }, [pageNumber]);

  useEffect(() => {
    if (!onClose) return;
    const onKey = (e) => { if (e.key === "Escape") onClose(); };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [onClose]);

  useEffect(() => {
    if (!pdfDoc || !canvasRef.current) return;
    let cancelled = false;

    (async () => {
      // Cancel any in-flight render on this canvas before starting a new one.
      if (renderTaskRef.current) {
        renderTaskRef.current.cancel();
        renderTaskRef.current = null;
      }

      const page = await pdfDoc.getPage(currentPage);
      if (cancelled) return;
      const viewport = page.getViewport({ scale: SCALE });
      const canvas = canvasRef.current;
      const ctx = canvas.getContext("2d");
      canvas.width = viewport.width;
      canvas.height = viewport.height;

      const task = page.render({ canvasContext: ctx, viewport });
      renderTaskRef.current = task;
      try {
        await task.promise;
      } catch (err) {
        // RenderingCancelledException is expected when superseded — swallow it.
        if (err?.name !== "RenderingCancelledException") throw err;
        return;
      }
      if (cancelled) return;

      if (bbox) {
        const pageHeight = viewport.viewBox[3];
        const cx1 = bbox.x1 * SCALE;
        const cy1 = (pageHeight - bbox.y2) * SCALE;
        const cw = (bbox.x2 - bbox.x1) * SCALE;
        const ch = (bbox.y2 - bbox.y1) * SCALE;
        const isSpecial = chunkType === "image" || chunkType === "table";
        ctx.fillStyle = isSpecial
          ? "rgba(59, 130, 246, 0.35)"
          : "rgba(250, 204, 21, 0.4)";
        ctx.fillRect(cx1, cy1, cw, ch);
        ctx.strokeStyle = isSpecial ? "rgba(37, 99, 235, 0.8)" : "rgba(202, 138, 4, 0.8)";
        ctx.lineWidth = 2;
        ctx.strokeRect(cx1, cy1, cw, ch);
      }
    })();

    return () => {
      cancelled = true;
      if (renderTaskRef.current) {
        renderTaskRef.current.cancel();
        renderTaskRef.current = null;
      }
    };
  }, [pdfDoc, currentPage, bbox, chunkType]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-48 text-gray-400 text-sm">
        Loading PDF…
      </div>
    );
  }
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center gap-3 h-48 text-red-400 text-sm">
        <span>{error}</span>
        {onClose && (
          <button
            onClick={onClose}
            className="px-3 py-1 text-xs rounded bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200"
          >
            Close
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-2 w-full min-w-0">
      <div className="flex w-full items-center justify-between gap-3 text-sm text-gray-600 dark:text-gray-300">
        <div className="flex items-center gap-3">
          <button
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={currentPage <= 1}
            className="px-2 py-0.5 rounded bg-gray-200 dark:bg-gray-700 disabled:opacity-40"
            aria-label="Previous page"
          >
            ‹
          </button>
          <span>Page {currentPage} / {totalPages}</span>
          <button
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={currentPage >= totalPages}
            className="px-2 py-0.5 rounded bg-gray-200 dark:bg-gray-700 disabled:opacity-40"
            aria-label="Next page"
          >
            ›
          </button>
        </div>
        {onClose && (
          <button
            onClick={onClose}
            className="rounded p-1 text-gray-500 hover:bg-gray-200 dark:text-gray-400 dark:hover:bg-gray-700"
            aria-label="Close PDF preview"
            title="Close (Esc)"
          >
            <X size={16} />
          </button>
        )}
      </div>

      <div className="w-full max-w-full overflow-auto border border-gray-200 dark:border-gray-700 rounded">
        <canvas ref={canvasRef} className="block max-w-full h-auto" />
      </div>

      {bbox && (
        <p className="text-xs text-gray-500 dark:text-gray-400">
          {chunkType === "image" || chunkType === "table"
            ? "Blue = image / table region"
            : "Yellow = source text region"}
        </p>
      )}
    </div>
  );
}
