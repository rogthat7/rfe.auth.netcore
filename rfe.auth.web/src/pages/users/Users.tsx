/* ─── Users Page ──────────────────────────────────────────────────────────── */
import { useState, useEffect } from 'react'
import { Badge, roleToBadge } from '../../components/ui/Badge/Badge'
import { useAppStore } from '../../store/app.store'
import { getInitials, formatPhone, formatRelativeTime, formatRole } from '../../utils/formatters'
import { userService } from '../../services/user.service'
import type { User } from '../../types/user.types'
import styles from './Users.module.css'

const APP_BADGE_COLORS: Record<string, 'blue'|'orange'|'grey'|'cyan'> = {
  'rfe-auth':   'blue',
  'rfe-admin':  'orange',
  'rfe-portal': 'cyan',
}

const APP_IDS = ['rfe-auth', 'rfe-admin', 'rfe-portal'] as const

export default function Users() {
  const selectedAppId = useAppStore((s: { selectedAppId: string }) => s.selectedAppId)
  const setApp        = useAppStore((s: { setSelectedApp: (id: string) => void }) => s.setSelectedApp)

  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    setLoading(true)
    userService.getUsers()
      .then((res) => {
        if (active) {
          const rawUsers = res.data || []
          const mapped: User[] = rawUsers.map((u: any) => {
            const appsArray = u.apps ? u.apps.split(',') : ['rfe-auth']
            return {
              id: u.userId,
              name: u.username || 'Unnamed User',
              phone: u.phone ? u.phone.toString() : '',
              role: u.role || 'Labourer',
              apps: appsArray,
              status: 'Active',
              createdAt: '',
              lastLoginAt: new Date().toISOString()
            }
          })
          setUsers(mapped)
          setError(null)
        }
      })
      .catch((err) => {
        if (active) {
          console.error('Failed to fetch users:', err)
          setError('Failed to load users from the server.')
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false)
        }
      })
    return () => {
      active = false
    }
  }, [])

  const filtered = users.filter((u) => u.apps.includes(selectedAppId))

  if (loading) {
    return <div className={styles.loading}>Loading users...</div>
  }

  if (error) {
    return <div className={styles.error}>{error}</div>
  }

  return (
    <div className={styles.page}>
      {/* App Context Banner */}
      <div className={styles.banner}>
        <div className={styles.bannerLeft}>
          <span className={styles.bannerDot} />
          <strong className={styles.bannerApp}>{selectedAppId}</strong>
          <span className={styles.bannerCount}>{filtered.length.toLocaleString()} users</span>
        </div>
        <div className={styles.bannerSwitcher}>
          {APP_IDS.map((id) => (
            <button
              key={id}
              className={[styles.switchBtn, id === selectedAppId ? styles.switchActive : ''].join(' ')}
              onClick={() => setApp(id)}
            >
              {id}
            </button>
          ))}
        </div>
      </div>

      <div className={styles.tableWrap}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th className={styles.th}><input type="checkbox" /></th>
              <th className={styles.th}>Name</th>
              <th className={styles.th}>Phone</th>
              <th className={styles.th}>Role</th>
              <th className={styles.th}>App Access</th>
              <th className={styles.th}>Last Login</th>
              <th className={styles.th}>Status</th>
              <th className={styles.th}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((user, i) => (
              <tr key={user.id} className={styles.tr} style={{ animationDelay: `${i * 40}ms` }}>
                <td className={styles.td}><input type="checkbox" /></td>
                <td className={styles.td}>
                  <div className={styles.nameCell}>
                    <div className={styles.avatar}>{getInitials(user.name)}</div>
                    <span className={styles.userName}>{user.name}</span>
                  </div>
                </td>
                <td className={styles.td}><span className={styles.phone}>{formatPhone(user.phone)}</span></td>
                <td className={styles.td}><Badge variant={roleToBadge(user.role)}>{formatRole(user.role)}</Badge></td>
                <td className={styles.td}>
                  <div className={styles.appChips}>
                    {user.apps.map((app) => (
                      <Badge key={app} variant={APP_BADGE_COLORS[app] ?? 'grey'}>{app}</Badge>
                    ))}
                  </div>
                </td>
                <td className={styles.td}><span className={styles.muted}>{user.lastLoginAt ? formatRelativeTime(user.lastLoginAt) : 'Never'}</span></td>
                <td className={styles.td}>
                  <span className={[styles.statusDot, user.status === 'Active' ? styles.dotActive : styles.dotInactive].join(' ')} />
                  <span className={styles.statusText}>{user.status}</span>
                </td>
                <td className={styles.td}>
                  <div className={styles.rowActions}>
                    <button className={styles.actionBtn} title="Edit">✎</button>
                    {user.role !== 'Admin' && (
                      <button className={[styles.actionBtn, styles.actionDanger].join(' ')} title="Delete">✕</button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className={styles.pagination}>
        <span className={styles.pageInfo}>Showing 1–{filtered.length} of {filtered.length}</span>
        <div className={styles.pageControls}>
          <button className={styles.pageBtn} disabled>‹ Prev</button>
          <button className={[styles.pageBtn, styles.pageBtnActive].join(' ')}>1</button>
          <button className={styles.pageBtn} disabled>Next ›</button>
        </div>
      </div>
    </div>
  )
}
