/* ─── Badge Component ────────────────────────────────────────────────────── */
import styles from './Badge.module.css'

type BadgeVariant = 'blue' | 'indigo' | 'orange' | 'green' | 'red' | 'grey' | 'cyan'

interface BadgeProps {
  children: React.ReactNode
  variant?: BadgeVariant
  dot?: boolean
  className?: string
}

/** Map roles to badge variants */
export function roleToBadge(role: string): BadgeVariant {
  const map: Record<string, BadgeVariant> = {
    Admin: 'orange', PanchayatAdmin: 'orange',
    Labourer: 'blue', JobCreator: 'indigo',
  }
  return map[role] ?? 'grey'
}

export function Badge({ children, variant = 'blue', dot = false, className = '' }: BadgeProps) {
  return (
    <span className={[styles.badge, styles[variant], className].filter(Boolean).join(' ')}>
      {dot && <span className={styles.dot} />}
      {children}
    </span>
  )
}
