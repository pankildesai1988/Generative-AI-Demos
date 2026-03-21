import api from "./client";

export const ingestDocument = (file, uploadedBy = "demo-user") => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("uploadedBy", uploadedBy);
  return api.post("/documents/ingest", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

export const getDocuments = () => api.get("/documents");

export const getDocumentById = (documentId) => api.get(`/documents/${documentId}`);
