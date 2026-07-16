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
      id:    payload.userId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
      name:  payload.userName || payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'System Admin',
      phone: payload.phone || '',
      role:  payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'appUser',
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
      const searchParams = new URLSearchParams(window.location.search)
      const returnUrl = searchParams.get('ReturnUrl')
      if (returnUrl) {
        window.location.href = returnUrl
      } else {
        navigate('/')
      }
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
      navigate('/verify-email', {
        state: {
          identifier: req.email,
          devLink:    pending.devLink,
        },
      })
    }
  }

  async function logout() {
    await authService.logout()
    storeLogout()
    navigate('/login')
  }

  return { token, user, isAuthenticated, login, register, logout }
}
