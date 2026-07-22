/* ─── TopBar ──────────────────────────────────────────────────────────────── */
import { Bell, Search } from 'lucide-react'
import { useLocation } from 'react-router-dom'
import { useAuthStore } from '@store/auth.store'
import { useAppStore }  from '@store/app.store'
import { getInitials }  from '@utils/formatters'
import { Button }       from '@components/ui/Button/Button'
import styles from './TopBar.module.css'

interface TopBarProps {
  title?: string
  actionLabel?: string
  onAction?: () => void
}

export function TopBar({ title = 'Dashboard', actionLabel, onAction }: TopBarProps) {
  const user          = useAuthStore((s) => s.user)
  const selectedAppId = useAppStore((s) => s.selectedAppId)
  const location      = useLocation()
  const searchParams  = new URLSearchParams(location.search)
  const view          = searchParams.get('view')
  const path          = location.pathname

  const renderBreadcrumb = () => {
    if (path === '/') {
      return <span className={styles.appChip}>Dashboard</span>
    }
    if (path === '/applications') {
      return <span className={styles.appChip}>Applications</span>
    }
    if (path === '/analytics') {
      return <span className={styles.appChip}>Analytics</span>
    }
    if (path === '/users') {
      const appFilter = searchParams.get('app') || 'all'
      if (appFilter === 'all') {
        return (
          <>
            <span className={styles.crumbBase}>User Management</span>
            <span className={styles.crumbSep}>›</span>
            <span className={styles.appChip}>All Users</span>
          </>
        )
      } else {
        return (
          <>
            <span className={styles.crumbBase}>User Management</span>
            <span className={styles.crumbSep}>›</span>
            <span className={styles.crumbBase}>All Users</span>
            <span className={styles.crumbSep}>›</span>
            <span className={styles.appChip}>{appFilter}</span>
          </>
        )
      }
    }
    if (path === '/roles') {
      return (
        <>
          <span className={styles.crumbBase}>System</span>
          <span className={styles.crumbSep}>›</span>
          <span className={styles.appChip}>Roles & Permissions</span>
        </>
      )
    }
    if (path === '/sessions') {
      return (
        <>
          <span className={styles.crumbBase}>System</span>
          <span className={styles.crumbSep}>›</span>
          <span className={styles.appChip}>Sessions</span>
        </>
      )
    }
    if (path === '/audit') {
      return (
        <>
          <span className={styles.crumbBase}>System</span>
          <span className={styles.crumbSep}>›</span>
          <span className={styles.appChip}>Audit Logs</span>
        </>
      )
    }
    if (path === '/settings') {
      return (
        <>
          <span className={styles.crumbBase}>System</span>
          <span className={styles.crumbSep}>›</span>
          <span className={styles.appChip}>Settings</span>
        </>
      )
    }
    return <span className={styles.appChip}>{title}</span>
  }

  const getSearchPlaceholder = () => {
    if (path === '/users') {
      const appFilter = searchParams.get('app') || 'all'
      if (appFilter === 'all') {
        return 'Search all users…'
      }
      return `Search users in ${appFilter}…`
    }
    return `Search users in ${selectedAppId}…`
  }

  return (
    <header className={styles.bar}>
      {/* Breadcrumb */}
      <div className={styles.breadcrumb}>
        {renderBreadcrumb()}
      </div>

      {/* Search */}
      <div className={styles.search}>
        <Search size={14} className={styles.searchIcon} />
        <input
          className={styles.searchInput}
          placeholder={getSearchPlaceholder()}
          type="search"
        />
      </div>

      {/* Right actions */}
      <div className={styles.actions}>
        {actionLabel && onAction && (
          <Button size="sm" onClick={onAction}>{actionLabel}</Button>
        )}
        <button className={styles.bell} aria-label="Notifications">
          <Bell size={18} />
          <span className={styles.bellDot} aria-hidden />
        </button>
        <div className={styles.avatar} title={user?.name}>
          {user ? getInitials(user.name) : 'SA'}
        </div>
      </div>
    </header>
  )
}
