/* ─── Applications Page ───────────────────────────────────────────────────── */
import { useState } from 'react'
import { AppWindow, Plus, Users, Activity, Shield } from 'lucide-react'
import { Modal }  from '@components/ui/Modal/Modal'
import { Button } from '@components/ui/Button/Button'
import { Badge }  from '@components/ui/Badge/Badge'
import styles from './Applications.module.css'

/* ── Mock data (replace with applicationService.getApplications()) ── */
const MOCK_APPS = [
  {
    appId: 'rfe-glam', displayName: 'Glam Community Platform',
    description: 'Connecting Labourers and Job Creators',
    status: 'Active', userCount: 8920, sessionCount: 342,
    roles: ['Labourer', 'JobCreator'], color: 'blue', createdAt: '12 Jan 2025',
  },
  {
    appId: 'rfe-admin', displayName: 'System Administration Portal',
    description: 'RFE internal admin interface',
    status: 'Active', userCount: 12, sessionCount: 5,
    roles: ['Admin', 'PanchayatAdmin'], color: 'orange', createdAt: '1 Jan 2025',
  },
  {
    appId: 'rfe-portal', displayName: 'Community Portal',
    description: 'Coming Soon — not yet configured',
    status: 'Inactive', userCount: 0, sessionCount: 0,
    roles: [], color: 'grey', createdAt: '25 Jun 2025',
  },
] as const

const ROLE_BADGE_MAP: Record<string, 'blue'|'indigo'|'orange'|'grey'> = {
  Labourer: 'blue', JobCreator: 'indigo', Admin: 'orange', PanchayatAdmin: 'orange',
}

export default function Applications() {
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <div className={styles.page}>
      {/* Header */}
      <div className={styles.header}>
        <div>
          <h2 className={styles.title}>Registered Applications</h2>
          <p className={styles.subtitle}>Manage client applications and their authorized users</p>
        </div>
        <Button onClick={() => setModalOpen(true)}>
          <Plus size={16} /> Register New App
        </Button>
      </div>

      {/* Summary chips */}
      <div className={styles.summary}>
        <span className={styles.chip}><AppWindow size={13} /> Total Apps: 3</span>
        <span className={styles.chip}><Users size={13} />    Total Users: 12,450</span>
        <span className={styles.chip}><Activity size={13} /> Active Sessions: 687</span>
      </div>

      {/* App Cards Grid */}
      <div className={styles.grid}>
        {MOCK_APPS.map((app) => (
          <div
            key={app.appId}
            className={[styles.card, styles[`card_${app.color}`], app.status === 'Inactive' ? styles.inactive : ''].filter(Boolean).join(' ')}
          >
            <div className={styles.cardHeader}>
              <div className={[styles.appIcon, styles[`icon_${app.color}`]].join(' ')}>
                <AppWindow size={20} />
              </div>
              <div className={styles.appMeta}>
                <span className={styles.appId}>{app.appId}</span>
                <span className={[styles.statusDot, app.status === 'Active' ? styles.dotActive : styles.dotInactive].join(' ')} />
              </div>
            </div>

            <p className={styles.appDesc}>{app.description}</p>

            <div className={styles.mono}>app_id: {app.appId}</div>

            <div className={styles.stats}>
              <div className={styles.stat}><Users size={13} /> {app.userCount.toLocaleString()} Users</div>
              <div className={styles.stat}><Activity size={13} /> {app.sessionCount} Sessions</div>
              <div className={styles.stat}><Shield size={13} /> {app.roles.length} Roles</div>
            </div>

            <div className={styles.roles}>
              {app.roles.length > 0
                ? app.roles.map((r) => (
                    <Badge key={r} variant={ROLE_BADGE_MAP[r] ?? 'grey'}>{r}</Badge>
                  ))
                : <span className={styles.noRoles}>Not configured</span>
              }
            </div>

            <div className={styles.cardActions}>
              <Button size="sm" disabled={app.status === 'Inactive'}>View Users</Button>
              <Button size="sm" variant="secondary" disabled={app.status === 'Inactive'}>Manage Roles</Button>
            </div>
          </div>
        ))}

        {/* Register New App card */}
        <button className={styles.addCard} onClick={() => setModalOpen(true)}>
          <div className={styles.addIcon}><Plus size={24} /></div>
          <span className={styles.addLabel}>Register New Application</span>
          <span className={styles.addSub}>Add a new client app to use RFE Auth API</span>
        </button>
      </div>

      {/* Register Modal */}
      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="Register Application">
        <form className={styles.modalForm} onSubmit={(e) => e.preventDefault()}>
          <div className={styles.field}>
            <label>App ID</label>
            <input className={styles.input} placeholder="e.g. rfe-portal" />
            <span className={styles.hint}>Lowercase letters, numbers, hyphens only</span>
          </div>
          <div className={styles.field}>
            <label>Display Name</label>
            <input className={styles.input} placeholder="e.g. Community Portal" />
          </div>
          <div className={styles.field}>
            <label>Description</label>
            <textarea className={styles.textarea} rows={2} placeholder="Brief description…" />
          </div>
          <div className={styles.field}>
            <label>Allowed Roles</label>
            <div className={styles.checkboxGroup}>
              {['Admin', 'PanchayatAdmin', 'Labourer', 'JobCreator'].map((r) => (
                <label key={r} className={styles.checkboxLabel}>
                  <input type="checkbox" className={styles.checkbox} /> {r}
                </label>
              ))}
            </div>
          </div>
          <div className={styles.field}>
            <label>Webhook URL <span className={styles.optional}>(optional)</span></label>
            <input className={styles.input} type="url" placeholder="https://…" />
          </div>
          <div className={styles.modalActions}>
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)}>Cancel</Button>
            <Button type="submit">Register App</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
