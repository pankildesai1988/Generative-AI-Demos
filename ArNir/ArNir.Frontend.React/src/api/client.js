import axios from "axios";
import { getRuntimeApiUrl } from "../config/runtime";

const api = axios.create({
  baseURL: getRuntimeApiUrl(),
  headers: { "Content-Type": "application/json" },
});

export default api;
