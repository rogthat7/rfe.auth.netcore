/* ─── Auth Store (Zustand) ────────────────────────────────────────────────── */
import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { CurrentUser } from '../types/auth.types'

interface AuthState {
  token: string | null
  user: CurrentUser | null
  isAuthenticated: boolean
  setAuth: (token: string, user: CurrentUser) => void
  logout: () => void
}

function decodeJwt(token: string): CurrentUser | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    console.log(payload);
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
      id: payload.userId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
      name: payload.userName || payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'Auth User',
      phone: payload.phone || '',
      role: payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'appUser',
      apps: appsArray,
    }
  } catch { return null }
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,
      setAuth: (token, user) => set({ token, user, isAuthenticated: true }),
      logout: () => set({ token: null, user: null, isAuthenticated: false }),
    }),
    {
      name: 'rfe-auth-store',
      partialize: (state) => ({ token: state.token }),
      onRehydrateStorage: () => (state) => {
        if (state?.token) {
          const user = decodeJwt(state.token)
          if (user) { state.isAuthenticated = true; state.user = user }
          else state.logout()
        }
      },
    }
  )
)
