/* ─── Login Page ──────────────────────────────────────────────────────────── */
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Phone, User } from 'lucide-react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'

import { AuthLayout }   from '../../../components/layout/AuthLayout/AuthLayout'
import { Button }       from '../../../components/ui/Button/Button'
import { Input, PhoneInput, PasswordInput } from '../../../components/ui/Input/Input'
import { useAuth }      from '../../../hooks/useAuth'
import { loginSchema, type LoginFormValues } from '../../../utils/validators'
import styles from './Login.module.css'

const APP_ID = 'rfe-auth'

export default function Login() {
  const { login }  = useAuth()
  const [loading, setLoading] = useState(false)

  const { control, register, handleSubmit, formState: { errors } } =
    useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  async function onSubmit(values: LoginFormValues) {
    setLoading(true)
    try {
      await login({ ...values, appId: APP_ID })
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })
                    ?.response?.data?.message ?? 'Login failed. Please try again.'
      toast.error(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout>
      <div className={styles.card}>
        <div className={styles.header}>
          <h1 className={styles.title}>Welcome Back</h1>
          <p className={styles.subtitle}>Sign in to continue to your workspace</p>
        </div>
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form} noValidate>
          <Controller
            name="phone"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <Input
                label="Username or Phone Number"
                id="phone"
                {...field}
                placeholder="admin or 9876543210"
                icon={<User size={16} />}
                error={errors.phone?.message}
              />
            )}
          />
          <PasswordInput
            label="Password"
            id="password"
            placeholder="Enter your password"
            error={errors.password?.message}
            {...register('password')}
          />
          <Button type="submit" fullWidth loading={loading}>Sign In</Button>
        </form>
        <p className={styles.footer}>
          Don't have an account? <Link to="/register">Register</Link>
        </p>
        <p className={styles.powered}>Powered by RFE Auth API</p>
      </div>
    </AuthLayout>
  )
}
