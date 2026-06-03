import { useState, useCallback } from "react";
import { uploadDocument } from "../api/documents";
import { trackEvent } from "../analytics/tracker";

const ALLOWED_TYPES = [
  "application/pdf",
  "text/plain",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
];
const MAX_BYTES = 20 * 1024 * 1024; // 20 MB

export function useFileUpload() {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);

  const validate = (f) => {
    if (!f) return "No file selected.";
    if (!ALLOWED_TYPES.includes(f.type)) return "Only PDF, TXT, and DOCX files allowed.";
    if (f.size > MAX_BYTES) return "File exceeds 20 MB limit.";
    return null;
  };

  const selectFile = useCallback((f) => {
    const err = validate(f);
    if (err) {
      setError(err);
      setFile(null);
      return false;
    }
    setError("");
    setFile(f);
    return true;
  }, []);

  const upload = useCallback(
    async (uploadedBy = "demo-user") => {
      if (!file) return;
      setUploading(true);
      setError("");
      setResult(null);
      trackEvent("upload", "submit", file.type, { sizeBytes: file.size });
      try {
        const res = await uploadDocument(file, uploadedBy);
        setResult(res.data);
        setFile(null);
        trackEvent("upload", "success", file.type, {});
        return res.data;
      } catch (e) {
        const msg = e?.response?.data?.message ?? "Upload failed.";
        setError(msg);
        trackEvent("upload", "error", file.type, { message: msg });
        throw e;
      } finally {
        setUploading(false);
      }
    },
    [file]
  );

  const onDrop = useCallback(
    (e) => {
      e.preventDefault();
      const dropped = e.dataTransfer?.files?.[0];
      if (dropped) selectFile(dropped);
    },
    [selectFile]
  );

  const onInputChange = useCallback(
    (e) => {
      const picked = e.target.files?.[0];
      if (picked) selectFile(picked);
    },
    [selectFile]
  );

  return { file, uploading, error, result, selectFile, upload, onDrop, onInputChange };
}
