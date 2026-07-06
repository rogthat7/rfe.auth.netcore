/* ─── App Store (Zustand) ─────────────────────────────────────────────────── */
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export interface AppState {
  selectedAppId: string
  sidebarCollapsed: boolean
  setSelectedApp: (appId: string) => void
  toggleSidebar: () => void
}

export const useAppStore = create<AppState>()(
  persist(
    (set) => ({
      selectedAppId:    'rfe-glam',
      sidebarCollapsed: false,
      setSelectedApp: (appId) => set({ selectedAppId: appId }),
      toggleSidebar:  ()     => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
    }),
    { name: 'rfe-app-store' }
  )
)
