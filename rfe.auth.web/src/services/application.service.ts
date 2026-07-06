/* ─── Application Service ─────────────────────────────────────────────────── */
import { api } from './api'
import type { Application, CreateApplicationRequest } from '../types/application.types'
import type { ApiResponse } from '../types/api.types'

export const applicationService = {
  async getApplications(): Promise<Application[]> {
    const res = await api.get<ApiResponse<Application[]>>('/api/applications')
    return res.data.data
  },
  async getApplicationById(appId: string): Promise<Application> {
    const res = await api.get<ApiResponse<Application>>(`/api/applications/${appId}`)
    return res.data.data
  },
  async createApplication(req: CreateApplicationRequest): Promise<Application> {
    const res = await api.post<ApiResponse<Application>>('/api/applications', req)
    return res.data.data
  },
  async deleteApplication(appId: string): Promise<void> {
    await api.delete(`/api/applications/${appId}`)
  },
}
