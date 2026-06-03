import { FileText, RefreshCcw } from "lucide-react";

export default function DocumentSelector({
  documents = [],
  selectedIds = [],
  loading = false,
  error = null,
  onToggle,
  onSelectAll,
  onClear,
  onRefresh,
}) {
  return (
    <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-900">
      <div className="flex items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <FileText className="text-primary-600 dark:text-primary-400" size={18} />
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100">Document Scope</h3>
          </div>
          <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Select one or more documents to constrain RAG answers.
          </p>
        </div>
        <button
          type="button"
          onClick={onRefresh}
          className="rounded-lg p-2 text-gray-500 transition hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"
          aria-label="Refresh document list"
        >
          <RefreshCcw size={16} />
        </button>
      </div>

      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          onClick={onSelectAll}
          className="rounded-full bg-primary-50 px-3 py-1.5 text-xs font-medium text-primary-700 dark:bg-primary-900/30 dark:text-primary-300"
        >
          Select all
        </button>
        <button
          type="button"
          onClick={onClear}
          className="rounded-full bg-gray-100 px-3 py-1.5 text-xs font-medium text-gray-600 dark:bg-gray-800 dark:text-gray-300"
        >
          Clear selection
        </button>
        <span className="rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300">
          {selectedIds.length === 0
            ? "No filter — all docs searched"
            : `${selectedIds.length} selected`}
        </span>
      </div>

      {loading ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">Loading available documents…</p>
      ) : error ? (
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      ) : documents.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">
          No ingested documents found yet. Upload a document first.
        </p>
      ) : (
        <div className="max-h-56 space-y-2 overflow-y-auto pr-1">
          {documents.map((doc) => {
            const checked = selectedIds.includes(doc.id);
            return (
              <label
                key={doc.id}
                className={`flex cursor-pointer items-start gap-3 rounded-xl border px-3 py-2.5 transition ${
                  checked
                    ? "border-primary-300 bg-primary-50 dark:border-primary-700 dark:bg-primary-900/20"
                    : "border-gray-200 hover:bg-gray-50 dark:border-gray-700 dark:hover:bg-gray-800/60"
                }`}
              >
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={() => onToggle(doc.id)}
                  className="mt-1 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium text-gray-900 dark:text-gray-100">
                    {doc.name || doc.fileName || doc.title}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    {doc.type || doc.contentType || "Unknown type"} ·{" "}
                    {doc.chunks?.length ?? doc.chunkCount ?? 0} chunks
                  </p>
                </div>
              </label>
            );
          })}
        </div>
      )}
    </div>
  );
}
