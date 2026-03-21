import { useCallback, useEffect, useState } from "react";
import { getDocuments } from "@arnir/shared";

export default function useDocumentList() {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const refreshDocuments = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await getDocuments();
      setDocuments(response.data || []);
    } catch (err) {
      setError(err.response?.data?.message || "Failed to load documents.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refreshDocuments();
  }, [refreshDocuments]);

  return { documents, loading, error, refreshDocuments };
}
