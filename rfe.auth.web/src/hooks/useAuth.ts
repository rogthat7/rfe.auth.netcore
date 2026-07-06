/* ─── useAuth Hook ────────────────────────────────────────────────────────── */
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/auth.store'
import { authService }  from '../services/auth.service'
import type { LoginRequest, RegisterRequest } from '../types/auth.types'
import { toast } from 'sonner'

function decodeJwt(token: string) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      id:    payload.sub,
      name:  payload.name,
      phone: payload.phone,
      role:  payload.role,
      apps:  Array.isArray(payload.apps) ? payload.apps : [],
    }
  } catch { return null }
}

export function useAuth() {
  const { token, user, isAuthenticated, setAuth, logout: storeLogout } = useAuthStore()
  const navigate = useNavigate()

  async function login(req: LoginRequest) {
    const res     = await authService.login(req)
    const decoded = decodeJwt(res.token)
    if (!decoded) throw new Error('Invalid token received')
    setAuth(res.token, decoded)
    toast.success(`Welcome back, ${decoded.name}!`)
    navigate('/')
  }

  async function register(req: RegisterRequest) {
    const res     = await authService.register(req)
    const decoded = decodeJwt(res.token)
    if (!decoded) throw new Error('Invalid token received')
    setAuth(res.token, decoded)
    toast.success(`Account created! Welcome, ${decoded.name}.`)
    navigate('/')
  }

  async function logout() {
    await authService.logout()
    storeLogout()
    navigate('/login')
  }

  return { token, user, isAuthenticated, login, register, logout }
}
