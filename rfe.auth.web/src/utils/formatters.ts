/* ─── Formatters ──────────────────────────────────────────────────────────── */

/**
 * Format a raw 10-digit phone number for display: "9876543210" → "98765 43210"
 */
export function formatPhone(raw: string): string {
  const digits = raw.replace(/\D/g, '').slice(0, 10)
  if (digits.length <= 5) return digits
  return `${digits.slice(0, 5)} ${digits.slice(5)}`
}

/**
 * Strip all spaces from a phone string before sending to API
 */
export function sanitizePhone(phone: string): string {
  return phone.replace(/\s/g, '')
}

/**
 * Format ISO date string to "6 Jul 2025"
 */
export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-IN', {
    day: 'numeric', month: 'short', year: 'numeric',
  })
}

/**
 * Format relative time: "2 mins ago", "Yesterday", "6 Jul 2025"
 */
export function formatRelativeTime(iso: string): string {
  const date  = new Date(iso)
  const now   = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60_000)
  const diffHrs  = Math.floor(diffMs / 3_600_000)
  const diffDays = Math.floor(diffMs / 86_400_000)

  if (diffMins < 1)   return 'Just now'
  if (diffMins < 60)  return `${diffMins} min${diffMins > 1 ? 's' : ''} ago`
  if (diffHrs  < 24)  return `${diffHrs} hr${diffHrs > 1 ? 's' : ''} ago`
  if (diffDays === 1) return 'Yesterday'
  return formatDate(iso)
}

/**
 * Get initials from a name: "Ravi Kumar" → "RK"
 */
export function getInitials(name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0].toUpperCase())
    .join('')
}

/**
 * Map a role key to a display label
 */
export function formatRole(role: string): string {
  const map: Record<string, string> = {
    Admin: 'Admin',
    PanchayatAdmin: 'Panchayat Admin',
    Labourer: 'Labourer',
    JobCreator: 'Job Creator',
  }
  return map[role] ?? role
}
