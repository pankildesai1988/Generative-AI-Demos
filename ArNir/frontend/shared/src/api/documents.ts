import api from "./client";
import type { AxiosResponse } from "axios";

export const ingestDocument = (file: File, uploadedBy: string = "demo-user"): Promise<AxiosResponse> => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("uploadedBy", uploadedBy);
  return api.post("/documents/ingest", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

export const getDocuments = (): Promise<AxiosResponse> => api.get("/documents");

export const getDocumentById = (documentId: string): Promise<AxiosResponse> =>
  api.get(`/documents/${documentId}`);
