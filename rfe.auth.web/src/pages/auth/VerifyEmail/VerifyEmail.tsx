import { useEffect, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'
import styles from './VerifyEmail.module.css'

export default function VerifyEmail() {
  const location     = useLocation()
  const [params]     = useSearchParams()
  const state = location.state as { identifier?: string } | null

  const status   = params.get('status')           // 'confirmed' when redirect from backend
  const confirmed = status === 'confirmed'

  useEffect(() => {
    if (confirmed) toast.success('Email verified! You can now log in.')
  }, [confirmed])

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        {confirmed ? (
          <>
            <div className={styles.iconCheck}>✓</div>
            <h1 className={styles.title}>Email Verified!</h1>
            <p className={styles.sub}>
              Your account has been confirmed. You can now log in.
            </p>
            <Link to="/login" id="go-to-login" className={styles.btn}>Go to Login</Link>
          </>
        ) : (
          <>
            <div className={styles.icon}>✉️</div>
            <h1 className={styles.title}>Check your inbox</h1>
            <p className={styles.sub}>
              We sent a verification link to
            </p>
            {state?.identifier && (
              <div className={styles.identifier}>{state.identifier}</div>
            )}
            <p className={styles.hint}>
              Click the link in the email to activate your account.
              <br />The link is valid for 24 hours.
            </p>
            <hr className={styles.divider} />
            <p className={styles.hint} style={{ marginBottom: 0 }}>
              Didn't receive it? Check your spam folder.
            </p>
            <Link to="/login" id="back-to-login" className={styles.back}>← Back to login</Link>
          </>
        )}
      </div>
    </div>
  )
}
