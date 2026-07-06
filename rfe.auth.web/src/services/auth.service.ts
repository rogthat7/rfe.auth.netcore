/* ─── Auth Service ────────────────────────────────────────────────────────── */
import { api } from './api'
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types/auth.types'
import type { ApiResponse } from '../types/api.types'

export const authService = {
  async login(req: LoginRequest): Promise<AuthResponse> {
    const payload = { ...req, phone: req.phone.replace(/\s/g, '') }
    const res = await api.post<ApiResponse<AuthResponse>>('/api/auth/login', payload)
    return res.data.data
  },
  async register(req: RegisterRequest): Promise<AuthResponse> {
    const payload = { ...req, phone: req.phone.replace(/\s/g, '') }
    const res = await api.post<ApiResponse<AuthResponse>>('/api/auth/register', payload)
    return res.data.data
  },
  async logout(): Promise<void> {
    try { await api.post('/api/auth/logout') } catch { /* ignore */ }
  },
}
