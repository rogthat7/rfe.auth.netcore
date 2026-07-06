/* ─── Input Component ─────────────────────────────────────────────────────── */
import { forwardRef, useState, type InputHTMLAttributes } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import styles from './Input.module.css'
import { formatPhone } from '@utils/formatters'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  icon?: React.ReactNode
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, icon, className = '', id, ...props }, ref) => (
    <div className={styles.wrapper}>
      {label && <label htmlFor={id} className={styles.label}>{label}</label>}
      <div className={[styles.inputWrap, error ? styles.hasError : ''].join(' ')}>
        {icon && <span className={styles.icon}>{icon}</span>}
        <input
          ref={ref}
          id={id}
          className={[styles.input, icon ? styles.withIcon : '', className].filter(Boolean).join(' ')}
          {...props}
        />
      </div>
      {error && <span className={styles.error}>{error}</span>}
    </div>
  )
)
Input.displayName = 'Input'

/* ── Phone Input: auto-formats as "XXXXX XXXXX" ── */
interface PhoneInputProps extends Omit<InputProps, 'onChange' | 'value'> {
  value: string
  onChange: (raw: string) => void
}

export function PhoneInput({ value, onChange, ...rest }: PhoneInputProps) {
  const displayed = formatPhone(value)

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const raw = e.target.value.replace(/\D/g, '').slice(0, 10)
    onChange(raw)
  }

  return <Input {...rest} value={displayed} onChange={handleChange} inputMode="numeric" maxLength={11} />
}

/* ── Password Input: show/hide toggle ── */
export function PasswordInput({ label, error, ...rest }: InputProps) {
  const [show, setShow] = useState(false)
  return (
    <div className={styles.wrapper}>
      {label && <label className={styles.label}>{label}</label>}
      <div className={[styles.inputWrap, error ? styles.hasError : ''].join(' ')}>
        <input
          type={show ? 'text' : 'password'}
          className={[styles.input, styles.withIcon, styles.withToggle].join(' ')}
          {...rest}
        />
        <button
          type="button"
          className={styles.toggle}
          onClick={() => setShow((v) => !v)}
          tabIndex={-1}
          aria-label={show ? 'Hide password' : 'Show password'}
        >
          {show ? <EyeOff size={16} /> : <Eye size={16} />}
        </button>
      </div>
      {error && <span className={styles.error}>{error}</span>}
    </div>
  )
}
