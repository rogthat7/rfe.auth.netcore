/* ─── Auth Service ────────────────────────────────────────────────────────── */
import { api } from './api'
import type {
  LoginRequest, RegisterRequest, AuthResponse,
  VerificationPending, VerifyPhoneRequest, ResendVerificationRequest
} from '../types/auth.types'
import type { ApiResponse } from '../types/api.types'

export class UnverifiedError extends Error {
  constructor(public identifier: string) {
    super('ACCOUNT_NOT_VERIFIED')
    this.name = 'UnverifiedError'
  }
}

export const authService = {
  async login(req: LoginRequest): Promise<AuthResponse> {
    const payload = {
      ...req,
      phone: req.phone?.replace(/\s/g, '') || undefined,
    }
    try {
      const res = await api.post<ApiResponse<AuthResponse>>('/api/auth/login', payload)
      return res.data.data
    } catch (err: any) {
      // 403 = user exists but not verified
      if (err?.response?.status === 403 && err.response.data?.verified === false) {
        const identifier = err.response.data.identifier || req.phone || req.email || ''
        throw new UnverifiedError(identifier)
      }
      throw err
    }
  },

  /** Register now returns a VerificationPending instead of a token */
  async register(req: RegisterRequest): Promise<VerificationPending> {
    const payload = { ...req, phone: req.phone ? req.phone.replace(/\s/g, '') : undefined }
    const res = await api.post<VerificationPending>('/api/auth/register', payload)
    return res.data as unknown as VerificationPending
  },

  /** Submit the 6-digit OTP to confirm phone */
  async verifyPhone(req: VerifyPhoneRequest): Promise<void> {
    await api.post('/api/auth/v1/user/confirmuserwithphone', req)
  },

  /** Resend a verification code */
  async resendVerification(req: ResendVerificationRequest): Promise<VerificationPending> {
    const res = await api.post<VerificationPending>('/api/auth/resend-verification', req)
    return res.data as unknown as VerificationPending
  },

  async logout(): Promise<void> {
    try { await api.post('/api/auth/logout') } catch { /* ignore */ }
  },
}
