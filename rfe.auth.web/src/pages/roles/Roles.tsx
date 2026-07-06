import { Shield } from 'lucide-react'
import { Button } from '@components/ui/Button/Button'
import { useNavigate } from 'react-router-dom'
import styles from './Roles.module.css'

const ROLE_DEFS = [
  { role: 'Admin',          apps: ['rfe-admin'],            color: 'orange', desc: 'Full system administration access. Not registerable via app.' },
  { role: 'PanchayatAdmin', apps: ['rfe-admin'],            color: 'orange', desc: 'Panchayat-level admin. Manages community data and approvals.' },
  { role: 'Labourer',       apps: ['rfe-glam'],             color: 'blue',   desc: 'App user. Can browse jobs and submit applications.' },
  { role: 'JobCreator',     apps: ['rfe-glam'],             color: 'indigo', desc: 'App user. Can post job listings and hire labourers.' },
]

export default function Roles() {
  const navigate = useNavigate()
  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h2 className={styles.title}>Roles &amp; Permissions</h2>
          <p className={styles.subtitle}>Manage role definitions and per-application access control</p>
        </div>
        <Button onClick={() => navigate('/applications')}>Manage Apps</Button>
      </div>
      <div className={styles.grid}>
        {ROLE_DEFS.map((r) => (
          <div key={r.role} className={[styles.card, styles[`c_${r.color}`]].join(' ')}>
            <div className={styles.roleIcon}><Shield size={20} /></div>
            <div className={styles.roleName}>{r.role}</div>
            <p className={styles.roleDesc}>{r.desc}</p>
            <div className={styles.appsRow}>
              {r.apps.map((a) => <span key={a} className={styles.appTag}>{a}</span>)}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
