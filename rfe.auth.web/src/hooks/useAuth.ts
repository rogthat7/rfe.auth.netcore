/* ─── useAuth Hook ────────────────────────────────────────────────────────── */
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/auth.store'
import { authService, UnverifiedError } from '../services/auth.service'
import type { LoginRequest, RegisterRequest } from '../types/auth.types'
import { toast } from 'sonner'

function decodeJwt(token: string) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    
    let appsArray: string[] = []
    if (payload.apps) {
      if (Array.isArray(payload.apps)) {
        appsArray = payload.apps
      } else {
        try {
          const parsed = JSON.parse(payload.apps)
          if (Array.isArray(parsed)) {
            appsArray = parsed
          }
        } catch {
          appsArray = [payload.apps]
        }
      }
    }

    return {
      id:    payload.userId || payload.sub || '',
      name:  payload.userName || payload.name || 'System Admin',
      phone: payload.userName || payload.phone || '',
      role:  payload.role || 'appUser',
      apps:  appsArray,
    }
  } catch { return null }
}

export function useAuth() {
  const { token, user, isAuthenticated, setAuth, logout: storeLogout } = useAuthStore()
  const navigate = useNavigate()

  async function login(req: LoginRequest) {
    try {
      const res     = await authService.login(req)
      const decoded = decodeJwt(res.token)
      if (!decoded) throw new Error('Invalid token received')
      setAuth(res.token, decoded)
      toast.success(`Welcome back, ${decoded.name}!`)
      navigate('/')
    } catch (err) {
      if (err instanceof UnverifiedError) {
        toast.warning('Account not verified. Please verify to continue.')
        navigate('/unverified', { state: { identifier: err.identifier } })
        return
      }
      throw err
    }
  }

  async function register(req: RegisterRequest) {
    const pending = await authService.register(req)
    if (pending.verificationMethod === 'phone') {
      toast.info('Verification code sent! Enter it below.')
      navigate('/verify-phone', {
        state: {
          tokenPayload: pending.tokenPayload,
          devOtp:       pending.devOtp,
          identifier:   req.phone,
          password:     req.password,
        },
      })
    } else {
      toast.info('Verification email sent! Check your inbox.')
      navigate('/verify-email', { state: { identifier: req.email } })
    }
  }

  async function logout() {
    await authService.logout()
    storeLogout()
    navigate('/login')
  }

  return { token, user, isAuthenticated, login, register, logout }
}
