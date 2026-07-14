/* ─── Applications Page ───────────────────────────────────────────────────── */
import { useState, useEffect } from 'react'
import { AppWindow, Plus, Users, Activity, Shield, Trash2 } from 'lucide-react'
import { Modal }  from '../../components/ui/Modal/Modal'
import { Button } from '../../components/ui/Button/Button'
import { Badge }  from '../../components/ui/Badge/Badge'
import { applicationService } from '../../services/application.service'
import type { Application } from '../../types/application.types'
import { toast } from 'sonner'
import styles from './Applications.module.css'

const ROLE_BADGE_MAP: Record<string, 'blue'|'indigo'|'orange'|'grey'> = {
  Labourer: 'blue', JobCreator: 'indigo', Admin: 'orange', PanchayatAdmin: 'orange',
  Laborer: 'blue', Employer: 'indigo'
}

const getAppColor = (appId: string) => {
  if (appId === 'rfe-auth') return 'blue'
  if (appId === 'rfe-admin') return 'orange'
  if (appId.includes('fish')) return 'indigo'
  return 'cyan'
}

export default function Applications() {
  const [modalOpen, setModalOpen] = useState(false)
  const [apps, setApps] = useState<Application[]>([])
  const [loading, setLoading] = useState(true)
  const [registering, setRegistering] = useState(false)

  // Form state
  const [appId, setAppId] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [description, setDescription] = useState('')
  const [allowedRoles, setAllowedRoles] = useState<string[]>([])
  const [webhookUrl, setWebhookUrl] = useState('')

  async function loadApps() {
    setLoading(true)
    try {
      const data = await applicationService.getApplications()
      setApps(data)
    } catch (err) {
      toast.error('Failed to load applications')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadApps()
  }, [])

  async function handleRegister(e: React.FormEvent) {
    e.preventDefault()
    if (!appId.trim() || !displayName.trim()) {
      toast.error('App ID and Display Name are required')
      return
    }
    setRegistering(true)
    try {
      await applicationService.createApplication({
        appId: appId.trim().toLowerCase(),
        displayName: displayName.trim(),
        description: description.trim(),
        allowedRoles: allowedRoles as any[],
        webhookUrl: webhookUrl.trim()
      })
      toast.success('Application registered successfully')
      setModalOpen(false)
      // reset form
      setAppId('')
      setDisplayName('')
      setDescription('')
      setAllowedRoles([])
      setWebhookUrl('')
      loadApps()
    } catch (err: any) {
      const msg = err.response?.data?.message ?? 'Failed to register application'
      toast.error(msg)
    } finally {
      setRegistering(false)
    }
  }

  async function handleDelete(id: string) {
    if (!confirm(`Are you sure you want to delete application "${id}"?`)) return
    try {
      await applicationService.deleteApplication(id)
      toast.success('Application deleted successfully')
      loadApps()
    } catch (err) {
      toast.error('Failed to delete application')
    }
  }

  const handleRoleToggle = (role: string) => {
    setAllowedRoles(prev => 
      prev.includes(role) ? prev.filter(r => r !== role) : [...prev, role]
    )
  }

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
        <span className={styles.chip}><AppWindow size={13} /> Total Apps: {apps.length}</span>
        <span className={styles.chip}><Users size={13} />    Total Users: {apps.reduce((sum, a) => sum + a.userCount, 0)}</span>
        <span className={styles.chip}><Activity size={13} /> Active Sessions: {apps.reduce((sum, a) => sum + (a.activeSessionCount || 0), 0)}</span>
      </div>

      {/* App Cards Grid */}
      <div className={styles.grid}>
        {loading ? (
          <div className={styles.loading}>Loading applications...</div>
        ) : (
          apps.map((app) => {
            const color = getAppColor(app.appId)
            return (
              <div
                key={app.appId}
                className={[styles.card, styles[`card_${color}`], app.status === 'Inactive' ? styles.inactive : ''].filter(Boolean).join(' ')}
              >
                <div className={styles.cardHeader}>
                  <div className={[styles.appIcon, styles[`icon_${color}`]].join(' ')}>
                    <AppWindow size={20} />
                  </div>
                  <div className={styles.appMeta}>
                    <span className={styles.appId}>{app.appId}</span>
                    <span className={[styles.statusDot, app.status === 'Active' ? styles.dotActive : styles.dotInactive].join(' ')} />
                  </div>
                </div>

                <h4 style={{ margin: '8px 0 4px', fontSize: '1.1rem', fontWeight: 600 }}>{app.displayName}</h4>
                <p className={styles.appDesc}>{app.description || 'No description provided.'}</p>

                <div className={styles.mono}>app_id: {app.appId}</div>

                <div className={styles.stats}>
                  <div className={styles.stat}><Users size={13} /> {app.userCount.toLocaleString()} Users</div>
                  <div className={styles.stat}><Activity size={13} /> {app.activeSessionCount ?? 0} Sessions</div>
                  <div className={styles.stat}><Shield size={13} /> {app.allowedRoles.length} Roles</div>
                </div>

                <div className={styles.roles}>
                  {app.allowedRoles.length > 0
                    ? app.allowedRoles.map((r) => (
                        <Badge key={r} variant={ROLE_BADGE_MAP[r] ?? 'grey'}>{r}</Badge>
                      ))
                    : <span className={styles.noRoles}>Not configured</span>
                  }
                </div>

                <div className={styles.cardActions}>
                  <Button size="sm" disabled={app.status === 'Inactive'}>View Users</Button>
                  <Button size="sm" variant="secondary" onClick={() => handleDelete(app.appId)} disabled={app.appId === 'rfe-auth'}>
                    <Trash2 size={13} /> Delete
                  </Button>
                </div>
              </div>
            )
          })
        )}

        {/* Register New App card */}
        <button className={styles.addCard} onClick={() => setModalOpen(true)}>
          <div className={styles.addIcon}><Plus size={24} /></div>
          <span className={styles.addLabel}>Register New Application</span>
          <span className={styles.addSub}>Add a new client app to use RFE Auth API</span>
        </button>
      </div>

      {/* Register Modal */}
      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="Register Application">
        <form className={styles.modalForm} onSubmit={handleRegister}>
          <div className={styles.field}>
            <label>App ID</label>
            <input 
              className={styles.input} 
              placeholder="e.g. rfe-portal" 
              value={appId}
              onChange={(e) => setAppId(e.target.value)}
              pattern="^[a-z0-9-]+$"
              title="Lowercase letters, numbers, hyphens only"
              required
            />
            <span className={styles.hint}>Lowercase letters, numbers, hyphens only</span>
          </div>
          <div className={styles.field}>
            <label>Display Name</label>
            <input 
              className={styles.input} 
              placeholder="e.g. Community Portal" 
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              required
            />
          </div>
          <div className={styles.field}>
            <label>Description</label>
            <textarea 
              className={styles.textarea} 
              rows={2} 
              placeholder="Brief description…" 
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className={styles.field}>
            <label>Allowed Roles</label>
            <div className={styles.checkboxGroup}>
              {['Admin', 'PanchayatAdmin', 'Labourer', 'JobCreator', 'Laborer', 'Employer'].map((r) => (
                <label key={r} className={styles.checkboxLabel}>
                  <input 
                    type="checkbox" 
                    className={styles.checkbox} 
                    checked={allowedRoles.includes(r)}
                    onChange={() => handleRoleToggle(r)}
                  /> {r}
                </label>
              ))}
            </div>
          </div>
          <div className={styles.field}>
            <label>Webhook URL <span className={styles.optional}>(optional)</span></label>
            <input 
              className={styles.input} 
              type="url" 
              placeholder="https://…" 
              value={webhookUrl}
              onChange={(e) => setWebhookUrl(e.target.value)}
            />
          </div>
          <div className={styles.modalActions}>
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)}>Cancel</Button>
            <Button type="submit" loading={registering}>Register App</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
