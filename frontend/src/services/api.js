import axios from "axios";
import { ref } from "vue";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5172/api",
  headers: { "Content-Type": "application/json" },
});

export const taskApi = {
  list: (params) => api.get("/tasks", { params }),
  get: (id) => api.get(`/tasks/${id}`),
  create: (task) => api.post("/tasks", task),
  update: (id, task) => api.put(`/tasks/${id}`, task),
  updateStatus: (id, status) => api.patch(`/tasks/${id}/status`, { status }),
  remove: (id) => api.delete(`/tasks/${id}`),
};

export const categoryApi = {
  list: () => api.get("/categories"),
  create: (category) => api.post("/categories", category),
};

export const assistantApi = {
  createTaskDraft: (prompt) => api.post("/tasks/ai-suggest", { prompt }),
};

// Shared app-level error state for failures that affect an optional service.
export const globalError = ref("");
export const setGlobalError = (message) => {
  globalError.value = message;
};
export const clearGlobalError = () => {
  globalError.value = "";
};

export function getApiError(error, fallback = "Something went wrong.") {
  return (
    error.response?.data?.message || error.response?.data?.title || fallback
  );
}
