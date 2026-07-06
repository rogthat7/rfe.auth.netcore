/* ─── TopBar ──────────────────────────────────────────────────────────────── */
import { Bell, Search } from 'lucide-react'
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

  return (
    <header className={styles.bar}>
      {/* Breadcrumb */}
      <div className={styles.breadcrumb}>
        <span className={styles.crumbBase}>Applications</span>
        <span className={styles.crumbSep}>›</span>
        <span className={styles.crumbCurrent}>{selectedAppId}</span>
        <span className={styles.appChip}>{title}</span>
      </div>

      {/* Search */}
      <div className={styles.search}>
        <Search size={14} className={styles.searchIcon} />
        <input
          className={styles.searchInput}
          placeholder={`Search users in ${selectedAppId}…`}
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
