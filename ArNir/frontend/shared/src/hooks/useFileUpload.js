import { useState, useCallback } from "react";
import { ingestDocument } from "../api/documents";
import { trackEvent } from "../analytics/tracker";

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
      trackEvent("upload", "error", file.name, {
        reason: "unsupported_type",
        type: file.type,
      });
      return;
    }

    setUploading(true);
    setError(null);
    setResult(null);
    trackEvent("upload", "submit", file.name, {
      type: file.type,
      size: file.size,
      uploadedBy,
    });

    try {
      const res = await ingestDocument(file, uploadedBy);
      setResult({
        message: res.data.message || "Document queued for processing.",
        documentId: res.data.documentId,
      });
      trackEvent("upload", "success", file.name, {
        type: file.type,
        size: file.size,
        uploadedBy,
        documentId: res.data.documentId ?? null,
      });
    } catch (err) {
      const errorMessage =
        err.response?.data?.message || "Upload failed. Please try again.";
      setError(errorMessage);
      trackEvent("upload", "error", file.name, {
        type: file.type,
        size: file.size,
        uploadedBy,
        message: errorMessage,
      });
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
