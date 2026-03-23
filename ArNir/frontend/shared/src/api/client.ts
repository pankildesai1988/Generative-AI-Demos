import axios, { type AxiosInstance } from "axios";
import { getRuntimeApiUrl } from "../config/runtime";

const api: AxiosInstance = axios.create({
  baseURL: getRuntimeApiUrl(),
  headers: { "Content-Type": "application/json" },
});

export default api;
