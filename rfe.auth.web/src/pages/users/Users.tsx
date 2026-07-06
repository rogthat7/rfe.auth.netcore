/* ─── Users Page ──────────────────────────────────────────────────────────── */
import { Badge, roleToBadge } from '../../../components/ui/Badge/Badge'
import { useAppStore, type AppState } from '../../../store/app.store'
import { getInitials, formatPhone, formatRelativeTime, formatRole } from '../../../utils/formatters'
import styles from './Users.module.css'

const MOCK_USERS = [
  { id:'1', name:'Ravi Kumar',  phone:'9876543210', role:'Labourer',   apps:['rfe-glam'],               status:'Active',   lastLoginAt: new Date(Date.now()-2*60000).toISOString()   },
  { id:'2', name:'Priya Sharma',phone:'8765432109', role:'JobCreator', apps:['rfe-glam'],               status:'Active',   lastLoginAt: new Date(Date.now()-15*60000).toISOString()  },
  { id:'3', name:'Amit Patel',  phone:'7654321098', role:'Labourer',   apps:['rfe-glam','rfe-portal'],  status:'Active',   lastLoginAt: new Date(Date.now()-3600000).toISOString()   },
  { id:'4', name:'Sunita Devi', phone:'6543210987', role:'JobCreator', apps:['rfe-glam'],               status:'Inactive', lastLoginAt: new Date(Date.now()-10800000).toISOString()  },
  { id:'5', name:'Mohan Singh', phone:'5432109876', role:'Labourer',   apps:['rfe-glam'],               status:'Active',   lastLoginAt: new Date(Date.now()-86400000).toISOString()  },
  { id:'6', name:'Dev Admin',   phone:'9999900000', role:'Admin',      apps:['rfe-glam','rfe-admin'],   status:'Active',   lastLoginAt: new Date(Date.now()-60000).toISOString()    },
]

const APP_BADGE_COLORS: Record<string, 'blue'|'orange'|'grey'|'cyan'> = {
  'rfe-glam':   'blue',
  'rfe-admin':  'orange',
  'rfe-portal': 'cyan',
}

const APP_IDS = ['rfe-glam', 'rfe-admin', 'rfe-portal'] as const

export default function Users() {
  const selectedAppId = useAppStore((s: { selectedAppId: string }) => s.selectedAppId)
  const setApp        = useAppStore((s: { setSelectedApp: (id: string) => void }) => s.setSelectedApp)

  const filtered = MOCK_USERS.filter((u) => u.apps.includes(selectedAppId))

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
                <td className={styles.td}><span className={styles.muted}>{formatRelativeTime(user.lastLoginAt)}</span></td>
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
