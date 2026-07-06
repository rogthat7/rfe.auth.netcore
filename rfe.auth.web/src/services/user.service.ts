/* ─── User Service ────────────────────────────────────────────────────────── */
import { api } from './api'
import type { User, CreateUserRequest, UpdateUserRequest } from '../types/user.types'
import type { ApiResponse, PaginatedResponse, PaginationParams } from '../types/api.types'

export const userService = {
  async getUsers(params?: PaginationParams): Promise<PaginatedResponse<User>> {
    const res = await api.get<PaginatedResponse<User>>('/api/users', { params })
    return res.data
  },
  async getUserById(id: string): Promise<User> {
    const res = await api.get<ApiResponse<User>>(`/api/users/${id}`)
    return res.data.data
  },
  async createUser(req: CreateUserRequest): Promise<User> {
    const payload = { ...req, phone: req.phone.replace(/\s/g, '') }
    const res = await api.post<ApiResponse<User>>('/api/users', payload)
    return res.data.data
  },
  async updateUser(id: string, req: UpdateUserRequest): Promise<User> {
    const res = await api.put<ApiResponse<User>>(`/api/users/${id}`, req)
    return res.data.data
  },
  async deleteUser(id: string): Promise<void> {
    await api.delete(`/api/users/${id}`)
  },
}
