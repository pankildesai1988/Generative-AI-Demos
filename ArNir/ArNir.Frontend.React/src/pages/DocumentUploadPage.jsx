import { useEffect, useState } from "react";
import { UploadCloud, FileText, CheckCircle } from "lucide-react";
import toast from "react-hot-toast";
import { useFileUpload } from "../hooks/useFileUpload";
import { listDocuments } from "../api/documents";

export default function DocumentUploadPage() {
  const { file, uploading, error, selectFile, upload, onDrop, onInputChange } =
    useFileUpload();
  const [docs, setDocs] = useState([]);
  const [dragOver, setDragOver] = useState(false);

  const fetchDocs = async () => {
    try {
      const res = await listDocuments();
      setDocs(res.data ?? []);
    } catch {
      // backend may not be running; silently ignore
    }
  };

  useEffect(() => {
    fetchDocs();
  }, []);

  const handleUpload = async () => {
    try {
      await upload();
      toast.success("Document queued for ingestion.");
      fetchDocs();
    } catch {
      toast.error(error || "Upload failed.");
    }
  };

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-2 text-gray-900 dark:text-white">
        Upload Documents
      </h1>
      <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">
        PDF, TXT, or DOCX — max 20 MB. Ingested into the RAG pipeline in the background.
      </p>

      {/* Drop zone */}
      <div
        onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => { setDragOver(false); onDrop(e); }}
        className={`border-2 border-dashed rounded-xl p-10 text-center transition-colors cursor-pointer ${
          dragOver
            ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
            : "border-gray-300 dark:border-gray-600 hover:border-blue-400 dark:hover:border-blue-500"
        }`}
        onClick={() => document.getElementById("file-input").click()}
      >
        <UploadCloud
          size={40}
          className={`mx-auto mb-3 ${dragOver ? "text-blue-500" : "text-gray-400"}`}
        />
        <p className="text-sm text-gray-600 dark:text-gray-300">
          Drag & drop a file here, or{" "}
          <span className="text-blue-600 dark:text-blue-400 font-medium">browse</span>
        </p>
        <p className="text-xs text-gray-400 mt-1">PDF · TXT · DOCX</p>
        <input
          id="file-input"
          type="file"
          accept=".pdf,.txt,.docx"
          className="hidden"
          onChange={onInputChange}
        />
      </div>

      {/* Selected file */}
      {file && (
        <div className="mt-4 flex items-center justify-between bg-gray-100 dark:bg-gray-700 rounded-lg px-4 py-3">
          <div className="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-200">
            <FileText size={18} className="text-blue-500" />
            {file.name}
            <span className="text-gray-400 text-xs">
              ({(file.size / 1024).toFixed(1)} KB)
            </span>
          </div>
          <button
            onClick={handleUpload}
            disabled={uploading}
            className="px-4 py-1.5 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {uploading ? "Uploading…" : "Upload"}
          </button>
        </div>
      )}

      {error && (
        <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>
      )}

      {/* Recent documents */}
      {docs.length > 0 && (
        <div className="mt-8">
          <h2 className="text-base font-semibold mb-3 text-gray-800 dark:text-gray-200">
            Recent Documents
          </h2>
          <ul className="space-y-2">
            {docs.slice(0, 10).map((doc) => (
              <li
                key={doc.id ?? doc.documentId}
                className="flex items-center gap-3 bg-white dark:bg-gray-800 border dark:border-gray-700 rounded-lg px-4 py-3 text-sm"
              >
                <CheckCircle size={16} className="text-green-500 flex-shrink-0" />
                <span className="flex-1 truncate text-gray-700 dark:text-gray-200">
                  {doc.fileName ?? doc.title ?? "Document"}
                </span>
                <span className="text-xs text-gray-400">
                  {doc.uploadedAt
                    ? new Date(doc.uploadedAt).toLocaleDateString()
                    : ""}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
