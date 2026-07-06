/* ─── Register Page ───────────────────────────────────────────────────────── */
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { User } from 'lucide-react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'

import { AuthLayout }    from '../../../components/layout/AuthLayout/AuthLayout'
import { Button }        from '../../../components/ui/Button/Button'
import { Input, PhoneInput, PasswordInput } from '../../../components/ui/Input/Input'
import { useAuth }       from '../../../hooks/useAuth'
import { registerSchema, type RegisterFormValues } from '../../../utils/validators'
import styles from './Register.module.css'

const APP_ID = 'rfe-glam'

const ALLOWED_ROLES = [
  { value: 'Labourer',   label: 'Labourer' },
  { value: 'JobCreator', label: 'Job Creator' },
] as const

export default function Register() {
  const { register: registerUser } = useAuth()
  const [loading, setLoading] = useState(false)

  const { control, register, handleSubmit, formState: { errors } } =
    useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) })

  async function onSubmit(values: RegisterFormValues) {
    setLoading(true)
    try {
      await registerUser({
        name:     values.name,
        phone:    values.phone,
        password: values.password,
        role:     values.role,
        appId:    APP_ID,
      })
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })
                    ?.response?.data?.message ?? 'Registration failed.'
      toast.error(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout tagline="Join Your Community Today">
      <div className={styles.card}>
        <div className={styles.header}>
          <h1 className={styles.title}>Create Account</h1>
          <p className={styles.subtitle}>Register as a community member</p>
        </div>
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form} noValidate>
          <Input
            label="Full Name"
            id="name"
            placeholder="Ravi Kumar"
            icon={<User size={16} />}
            error={errors.name?.message}
            {...register('name')}
          />
          <Controller
            name="phone"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <PhoneInput
                label="Phone Number"
                id="phone"
                value={field.value}
                onChange={field.onChange}
                placeholder="98765 43210"
                error={errors.phone?.message}
              />
            )}
          />
          <PasswordInput
            label="Password"
            id="password"
            placeholder="Create a password"
            error={errors.password?.message}
            {...register('password')}
          />
          <PasswordInput
            label="Confirm Password"
            id="confirmPassword"
            placeholder="Repeat your password"
            error={errors.confirmPassword?.message}
            {...register('confirmPassword')}
          />
          <div className={styles.selectWrap}>
            <label htmlFor="role" className={styles.label}>Select Role</label>
            <select
              id="role"
              className={[styles.select, errors.role ? styles.selectError : ''].join(' ')}
              {...register('role')}
            >
              <option value="">Choose your role…</option>
              {ALLOWED_ROLES.map((r) => (
                <option key={r.value} value={r.value}>{r.label}</option>
              ))}
            </select>
            {errors.role && <span className={styles.error}>{String(errors.role.message)}</span>}
          </div>
          <Button type="submit" fullWidth loading={loading}>Create Account</Button>
        </form>
        <p className={styles.footer}>
          Already have an account? <Link to="/login">Sign In</Link>
        </p>
        <p className={styles.powered}>Powered by RFE Auth API</p>
      </div>
    </AuthLayout>
  )
}
