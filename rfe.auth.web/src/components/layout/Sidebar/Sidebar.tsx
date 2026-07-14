/* ─── Sidebar ─────────────────────────────────────────────────────────────── */
import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard, AppWindow, BarChart3,
  Users, Layers, Shield, Activity, ScrollText, Settings,
  LogOut,
} from 'lucide-react'
import { useAuthStore } from '@store/auth.store'
import { useAuth }      from '../../../hooks/useAuth'
import { getInitials } from '@utils/formatters'
import styles from './Sidebar.module.css'

interface NavItem { label: string; to: string; icon: React.ReactNode }

const OVERVIEW: NavItem[] = [
  { label: 'Dashboard',    to: '/',             icon: <LayoutDashboard size={17} /> },
  { label: 'Applications', to: '/applications', icon: <AppWindow size={17} /> },
  { label: 'Analytics',    to: '/analytics',    icon: <BarChart3 size={17} /> },
]
const USER_MGMT: NavItem[] = [
  { label: 'All Users',       to: '/users',    icon: <Users size={17} /> },
  { label: 'By Application',  to: '/users?view=by-app', icon: <Layers size={17} /> },
]
const SYSTEM: NavItem[] = [
  { label: 'Roles & Permissions', to: '/roles',    icon: <Shield size={17} /> },
  { label: 'Sessions',            to: '/sessions', icon: <Activity size={17} /> },
  { label: 'Audit Logs',          to: '/audit',    icon: <ScrollText size={17} /> },
  { label: 'Settings',            to: '/settings', icon: <Settings size={17} /> },
]

function NavSection({ label, items }: { label: string; items: NavItem[] }) {
  return (
    <div className={styles.section}>
      <span className={styles.sectionLabel}>{label}</span>
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.to === '/'}
          className={({ isActive }) =>
            [styles.navItem, isActive ? styles.active : ''].filter(Boolean).join(' ')
          }
        >
          <span className={styles.navIcon}>{item.icon}</span>
          <span className={styles.navLabel}>{item.label}</span>
          {/* Active indicator arrow */}
        </NavLink>
      ))}
    </div>
  )
}

export function Sidebar() {
  const user = useAuthStore((s) => s.user)
  const { logout } = useAuth()

  return (
    <aside className={styles.sidebar}>
      {/* ── Brand ── */}
      <div className={styles.brand}>
        <div className={styles.brandIcon}>
          <Shield size={20} strokeWidth={2.5} />
        </div>
        <div className={styles.brandText}>
          <span className={styles.brandName}>RFE Auth API</span>
          <span className={styles.brandVersion}>v2.1</span>
        </div>
      </div>

      <div className={styles.divider} />

      {/* ── Nav ── */}
      <nav className={styles.nav}>
        <NavSection label="Overview"         items={OVERVIEW} />
        <NavSection label="User Management"  items={USER_MGMT} />
        <NavSection label="System"           items={SYSTEM} />
      </nav>

      {/* ── User Profile ── */}
      <div className={styles.profile}>
        <div className={styles.avatar}>{user ? getInitials(user.name) : 'SA'}</div>
        <div className={styles.profileInfo}>
          <span className={styles.profileName}>{user?.name ?? 'Super Admin'}</span>
          <span className={styles.profileRole}>SYSTEM</span>
        </div>
        <button
          onClick={(e) => {
            e.stopPropagation()
            logout()
          }}
          className={styles.logoutButton}
          title="Log Out"
          id="logout-btn"
        >
          <LogOut size={16} className={styles.logoutIcon} />
        </button>
      </div>
    </aside>
  )
}
