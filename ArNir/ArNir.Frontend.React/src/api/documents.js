import api from "./client";

export const uploadDocument = (file, uploadedBy = "demo-user") => {
  const form = new FormData();
  form.append("file", file);
  form.append("uploadedBy", uploadedBy);
  return api.post("/documents/ingest", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

export const listDocuments = () => api.get("/documents");

export const getDocument = (id) => api.get(`/documents/${id}`);
