import { useState, useCallback } from "react";
import { ingestDocument } from "../api/documents";

/**
 * Shared file upload hook for all demo frontends.
 * Handles drag-and-drop + manual file selection, calls document ingest API.
 */
export default function useFileUpload() {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);
  const [result, setResult] = useState(null);

  const uploadFile = useCallback(async (file, uploadedBy = "demo-user") => {
    if (!file) return;

    // Validate file type
    const allowedTypes = [
      "application/pdf",
      "text/plain",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    ];
    if (!allowedTypes.includes(file.type)) {
      setError("Only PDF, TXT, and DOCX files are supported.");
      return;
    }

    setUploading(true);
    setError(null);
    setResult(null);

    try {
      const res = await ingestDocument(file, uploadedBy);
      setResult({
        message: res.data.message || "Document queued for processing.",
        documentId: res.data.documentId,
      });
    } catch (err) {
      setError(
        err.response?.data?.message || "Upload failed. Please try again."
      );
    } finally {
      setUploading(false);
    }
  }, []);

  const reset = useCallback(() => {
    setError(null);
    setResult(null);
  }, []);

  return { uploadFile, uploading, error, result, reset };
}
