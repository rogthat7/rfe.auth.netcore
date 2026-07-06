/* ─── Dashboard Page ──────────────────────────────────────────────────────── */
import { Users, Activity, Hammer, Briefcase } from 'lucide-react'
import { useAuthStore } from '@store/auth.store'
import styles from './Dashboard.module.css'

interface StatCardProps {
  icon: React.ReactNode
  label: string
  value: string
  trend?: string
  color?: string
}

function StatCard({ icon, label, value, trend, color = 'blue' }: StatCardProps) {
  return (
    <div className={[styles.statCard, styles[`color_${color}`]].join(' ')}>
      <div className={styles.statIcon}>{icon}</div>
      <div className={styles.statInfo}>
        <span className={styles.statValue}>{value}</span>
        <span className={styles.statLabel}>{label}</span>
        {trend && <span className={styles.statTrend}>{trend}</span>}
      </div>
    </div>
  )
}

export default function Dashboard() {
  const user = useAuthStore((s) => s.user)

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h2 className={styles.title}>Dashboard</h2>
          <p className={styles.subtitle}>Welcome back, {user?.name ?? 'Admin'}</p>
        </div>
      </div>

      <div className={styles.grid}>
        <StatCard icon={<Users size={22} />}    label="Total Users"     value="12,450" trend="↑ 8% this week" color="blue"   />
        <StatCard icon={<Activity size={22} />} label="Active Sessions" value="687"    trend="● Live"         color="green"  />
        <StatCard icon={<Hammer size={22} />}   label="Labourers"       value="8,920"  color="indigo"  />
        <StatCard icon={<Briefcase size={22} />}label="Job Creators"    value="3,530"  color="cyan"    />
      </div>

      <div className={styles.quickLinks}>
        <h3 className={styles.sectionTitle}>Quick Access</h3>
        <div className={styles.linkGrid}>
          {[
            { href: '/applications', label: 'Manage Applications', desc: 'Register and configure client apps' },
            { href: '/users',        label: 'User Directory',       desc: 'Browse users across all apps' },
            { href: '/roles',        label: 'Roles & Permissions',  desc: 'Configure role-based access control' },
            { href: '/audit',        label: 'Audit Logs',           desc: 'Track authentication events' },
          ].map((l) => (
            <a key={l.href} href={l.href} className={styles.linkCard}>
              <span className={styles.linkLabel}>{l.label}</span>
              <span className={styles.linkDesc}>{l.desc}</span>
            </a>
          ))}
        </div>
      </div>
    </div>
  )
}
