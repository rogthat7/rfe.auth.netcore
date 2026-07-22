import { useState, useRef, useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { authService } from '../../../services/auth.service'
import { toast } from 'sonner'
import styles from './VerifyPhone.module.css'

export default function VerifyPhone() {
  const location = useLocation()
  const navigate = useNavigate()
  const state = location.state as {
    tokenPayload?: string
    devOtp?: string
    identifier?: string
    password?: string
  } | null

  const [otp, setOtp]       = useState(['', '', '', '', '', ''])
  const [error, setError]   = useState('')
  const [loading, setLoading] = useState(false)
  const [resendCooldown, setResendCooldown] = useState(0)
  const inputRefs = useRef<Array<HTMLInputElement | null>>([])

  // Countdown for resend button
  useEffect(() => {
    if (resendCooldown <= 0) return
    const t = setTimeout(() => setResendCooldown(c => c - 1), 1000)
    return () => clearTimeout(t)
  }, [resendCooldown])

  function handleChange(idx: number, val: string) {
    if (!/^\d?$/.test(val)) return
    const next = [...otp]
    next[idx] = val
    setOtp(next)
    if (val && idx < 5) inputRefs.current[idx + 1]?.focus()
  }

  function handleKeyDown(idx: number, e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Backspace' && !otp[idx] && idx > 0) {
      inputRefs.current[idx - 1]?.focus()
    }
    if (e.key === 'ArrowLeft' && idx > 0) inputRefs.current[idx - 1]?.focus()
    if (e.key === 'ArrowRight' && idx < 5) inputRefs.current[idx + 1]?.focus()
  }

  function handlePaste(e: React.ClipboardEvent) {
    const text = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
    if (text.length === 6) {
      setOtp(text.split(''))
      inputRefs.current[5]?.focus()
    }
    e.preventDefault()
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const code = otp.join('')
    if (code.length < 6) { setError('Please enter all 6 digits.'); return }
    if (!state?.tokenPayload) { setError('Session expired. Please register again.'); return }

    setLoading(true)
    setError('')
    try {
      await authService.verifyPhone({ tokenPayload: state.tokenPayload, code })
      toast.success('Phone verified! You can now log in.')
      navigate('/login')
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data || 'Verification failed. Please try again.'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }

  async function handleResend() {
    if (!state?.identifier) return
    setResendCooldown(60)
    try {
      const pending = await authService.resendVerification({
        identifier: state.identifier,
        method: 'phone',
        password: state.password
      })
      toast.info('New code sent!')
      // Update tokenPayload in location state
      navigate('/verify-phone', {
        replace: true,
        state: { ...state, tokenPayload: pending.tokenPayload, devOtp: pending.devOtp }
      })
    } catch {
      toast.error('Could not resend. Please try again.')
    }
  }

  const code = otp.join('')

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.icon}>📱</div>
        <h1 className={styles.title}>Verify your phone</h1>
        <p className={styles.sub}>
          We sent a 6-digit code to{' '}
          <strong>{state?.identifier || 'your phone'}</strong>.
          <br />It expires in 10 minutes.
        </p>

        {state?.devOtp && (
          <div className={styles.devBanner}>
            ⚡ SMS not configured — dev OTP:
            <span className={styles.devCode}>{state.devOtp}</span>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className={styles.otpRow} onPaste={handlePaste}>
            {otp.map((digit, i) => (
              <input
                key={i}
                ref={el => { inputRefs.current[i] = el }}
                id={`otp-${i}`}
                className={styles.otpBox}
                type="text"
                inputMode="numeric"
                maxLength={1}
                value={digit}
                onChange={e => handleChange(i, e.target.value)}
                onKeyDown={e => handleKeyDown(i, e)}
                autoFocus={i === 0}
              />
            ))}
          </div>

          {error && <div className={styles.error}>⚠ {error}</div>}

          <button
            className={styles.btn}
            type="submit"
            disabled={loading || code.length < 6}
            id="verify-phone-submit"
          >
            {loading ? 'Verifying…' : 'Verify & Continue'}
          </button>
        </form>

        <div className={styles.resend}>
          Didn't get the code?{' '}
          <button
            className={styles.resendBtn}
            onClick={handleResend}
            disabled={resendCooldown > 0}
            id="resend-otp-btn"
          >
            {resendCooldown > 0 ? `Resend in ${resendCooldown}s` : 'Resend'}
          </button>
        </div>

        <Link to="/login" className={styles.back}>← Back to login</Link>
      </div>
    </div>
  )
}
