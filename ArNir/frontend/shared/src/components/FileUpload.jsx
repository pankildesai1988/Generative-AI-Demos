import { useState, useRef } from "react";
import { Upload, FileText, CheckCircle, AlertCircle, X } from "lucide-react";

/**
 * Shared drag-and-drop file upload component.
 * Calls useFileUpload hook functions passed via props.
 */
export default function FileUpload({
  onUpload,
  uploading,
  error,
  result,
  onReset,
  acceptedTypes = ".pdf,.txt,.docx",
  guidance = "Drag and drop a document here, or click to browse",
}) {
  const [dragOver, setDragOver] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);
  const inputRef = useRef(null);

  const handleDrop = (e) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file) setSelectedFile(file);
  };

  const handleFileSelect = (e) => {
    const file = e.target.files[0];
    if (file) setSelectedFile(file);
  };

  const handleUpload = () => {
    if (selectedFile) {
      onUpload(selectedFile);
    }
  };

  const handleClear = () => {
    setSelectedFile(null);
    onReset?.();
    if (inputRef.current) inputRef.current.value = "";
  };

  return (
    <div className="space-y-4">
      {/* Drop Zone */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
        onClick={() => inputRef.current?.click()}
        className={`border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors ${
          dragOver
            ? "border-primary-500 bg-primary-50 dark:bg-primary-900/20"
            : "border-gray-300 dark:border-gray-600 hover:border-primary-400 hover:bg-gray-50 dark:hover:bg-gray-800"
        }`}
      >
        <Upload
          className={`mx-auto mb-3 ${
            dragOver ? "text-primary-500" : "text-gray-400 dark:text-gray-500"
          }`}
          size={40}
        />
        <p className="text-sm text-gray-600 dark:text-gray-400">{guidance}</p>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
          Supported: PDF, TXT, DOCX
        </p>
        <input
          ref={inputRef}
          type="file"
          accept={acceptedTypes}
          onChange={handleFileSelect}
          className="hidden"
        />
      </div>

      {/* Selected File */}
      {selectedFile && !result && (
        <div className="flex items-center gap-3 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border dark:border-gray-700">
          <FileText className="text-primary-600 dark:text-primary-400" size={20} />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium truncate dark:text-gray-200">{selectedFile.name}</p>
            <p className="text-xs text-gray-400 dark:text-gray-500">
              {(selectedFile.size / 1024).toFixed(1)} KB
            </p>
          </div>
          <button
            onClick={handleClear}
            className="text-gray-400 hover:text-red-500"
          >
            <X size={16} />
          </button>
          <button
            onClick={handleUpload}
            disabled={uploading}
            className="bg-primary-600 text-white px-4 py-1.5 rounded-lg text-sm hover:bg-primary-700 disabled:opacity-50 transition"
          >
            {uploading ? "Uploading..." : "Upload"}
          </button>
        </div>
      )}

      {/* Success */}
      {result && (
        <div className="flex items-center gap-3 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
          <CheckCircle className="text-green-500" size={20} />
          <div className="flex-1">
            <p className="text-sm font-medium text-green-700 dark:text-green-400">
              {result.message}
            </p>
            {result.documentId && (
              <p className="text-xs text-green-500 dark:text-green-600">
                Document ID: {result.documentId}
              </p>
            )}
          </div>
          <button
            onClick={handleClear}
            className="text-sm text-primary-600 dark:text-primary-400 hover:underline"
          >
            Upload Another
          </button>
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="flex items-center gap-3 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <AlertCircle className="text-red-500" size={20} />
          <p className="text-sm text-red-700 dark:text-red-400 flex-1">{error}</p>
          <button
            onClick={handleClear}
            className="text-sm text-red-600 dark:text-red-400 hover:underline"
          >
            Try Again
          </button>
        </div>
      )}
    </div>
  );
}
