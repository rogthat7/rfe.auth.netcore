/* ─── User Service ────────────────────────────────────────────────────────── */
import { api } from './api'
import type { User, CreateUserRequest, UpdateUserRequest } from '../types/user.types'
import type { ApiResponse, PaginatedResponse, PaginationParams } from '../types/api.types'

export const userService = {
  async getUsers(params?: PaginationParams): Promise<any> {
    const res = await api.get<any>('/api/auth/v1/User', { params })
    return res.data
  },
  async getUserById(id: string): Promise<any> {
    const res = await api.get<any>(`/api/auth/v1/User/${id}`)
    return res.data.data
  },
  async createUser(req: CreateUserRequest): Promise<any> {
    const payload = { ...req, phone: req.phone.replace(/\s/g, '') }
    const res = await api.post<any>('/api/auth/v1/User', payload)
    return res.data.data
  },
  async updateUser(id: string, req: UpdateUserRequest): Promise<any> {
    const res = await api.put<any>(`/api/auth/v1/User/${id}`, req)
    return res.data.data
  },
  async deleteUser(id: string): Promise<void> {
    await api.delete(`/api/auth/v1/User/${id}`)
  },
}
