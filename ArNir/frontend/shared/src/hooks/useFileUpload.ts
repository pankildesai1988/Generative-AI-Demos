import { useState, useCallback } from "react";
import { ingestDocument } from "../api/documents";
import { trackEvent } from "../analytics/tracker";
import type { FileUploadResult, FileUploadHookReturn } from "../types";

export default function useFileUpload(): FileUploadHookReturn {
  const [uploading, setUploading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<FileUploadResult | null>(null);

  const uploadFile = useCallback(async (file: File, uploadedBy: string = "demo-user") => {
    if (!file) return;

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
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      const errorMessage =
        axiosErr.response?.data?.message || "Upload failed. Please try again.";
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
