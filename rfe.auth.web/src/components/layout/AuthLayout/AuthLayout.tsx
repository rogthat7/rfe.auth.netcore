/* ─── AuthLayout — 40/60 Split ────────────────────────────────────────────── */
import type { ReactNode } from 'react'
import { Shield } from 'lucide-react'
import styles from './AuthLayout.module.css'

interface AuthLayoutProps {
  children: ReactNode
  title?: string
  tagline?: string
}

const STATS = [
  { value: '12K+',  label: 'Registered Users' },
  { value: '450+',  label: 'Panchayats Connected' },
  { value: '98%',   label: 'API Uptime' },
]

export function AuthLayout({ children, tagline = 'Connecting Communities, Creating Opportunities' }: AuthLayoutProps) {
  return (
    <div className={styles.root}>
      {/* ── Left Panel 40% ── */}
      <div className={styles.left}>
        <div className={styles.leftInner}>
          {/* Brand */}
          <div className={styles.brand}>
            <div className={styles.brandIcon}><Shield size={28} strokeWidth={2.5} /></div>
            <div>
              <div className={styles.brandName}>RFE Auth</div>
              <div className={styles.brandSub}>Auth API</div>
            </div>
          </div>

          {/* Tagline */}
          <p className={styles.tagline}>{tagline}</p>

          {/* Stat Cards */}
          <div className={styles.stats}>
            {STATS.map((s, i) => (
              <div key={i} className={styles.statCard}>
                <span className={styles.statValue}>{s.value}</span>
                <span className={styles.statLabel}>{s.label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Background grid pattern */}
        <div className={styles.grid} aria-hidden />
      </div>

      {/* ── Right Panel 60% ── */}
      <div className={styles.right}>
        <div className={styles.formContainer}>{children}</div>
      </div>
    </div>
  )
}
