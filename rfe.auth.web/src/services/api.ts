/* ─── Axios Base Instance ─────────────────────────────────────────────────── */
import axios from 'axios'
import { useAuthStore } from '../store/auth.store'

const BASE_URL = (import.meta as { env: { VITE_AUTH_API_URL?: string } }).env.VITE_AUTH_API_URL || '/'

export const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 10_000,
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const isLoginRequest = error.config?.url?.includes('/api/auth/login')
      if (!isLoginRequest) {
        useAuthStore.getState().logout()
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)
