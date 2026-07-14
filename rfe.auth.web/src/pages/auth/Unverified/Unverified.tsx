import { useState, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { authService } from '../../../services/auth.service'
import { toast } from 'sonner'
import styles from './Unverified.module.css'

export default function Unverified() {
  const location = useLocation()
  const state = location.state as { identifier?: string } | null
  const identifier = state?.identifier || ''

  // Derive method from identifier shape
  const method = /^\+?\d[\d\s]{6,}$/.test(identifier) ? 'phone' : 'email'

  const [resendCooldown, setResendCooldown] = useState(0)
  const [resendStatus, setResendStatus]     = useState('')

  useEffect(() => {
    if (resendCooldown <= 0) return
    const t = setTimeout(() => setResendCooldown(c => c - 1), 1000)
    return () => clearTimeout(t)
  }, [resendCooldown])

  async function handleResend() {
    if (!identifier) return
    setResendCooldown(60)
    setResendStatus('')
    try {
      await authService.resendVerification({ identifier, method })
      toast.info('Verification resent!')
      setResendStatus(
        method === 'phone'
          ? 'A new code has been sent to your phone.'
          : 'A new verification link has been sent to your email.'
      )
    } catch {
      toast.error('Could not resend. Please try again.')
      setResendCooldown(0)
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.iconWrap}>🔒</div>

        <h1 className={styles.title}>Account Not Verified</h1>
        <p className={styles.sub}>
          Your account has not been verified yet.
          {identifier && (
            <> You registered with:</>
          )}
        </p>

        {identifier && (
          <div className={styles.identifier}>{identifier}</div>
        )}

        <p className={styles.sub}>
          You must verify your{' '}
          <strong>{method === 'phone' ? 'phone number' : 'email address'}</strong>{' '}
          before you can log in.
        </p>

        <hr className={styles.divider} />

        <div className={styles.actions}>
          <button
            className={styles.btnPrimary}
            onClick={handleResend}
            disabled={resendCooldown > 0}
            id="resend-verification-btn"
          >
            {resendCooldown > 0
              ? `Resend in ${resendCooldown}s`
              : method === 'phone'
                ? 'Resend Verification Code'
                : 'Resend Verification Email'}
          </button>

          <Link to="/login" className={styles.btnGhost} id="back-to-login-unverified">
            ← Back to Login
          </Link>
        </div>

        {resendStatus && <p className={styles.resendStatus}>{resendStatus}</p>}
      </div>
    </div>
  )
}
