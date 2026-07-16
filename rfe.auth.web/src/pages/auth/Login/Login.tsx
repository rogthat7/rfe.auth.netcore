/* ─── Login Page ──────────────────────────────────────────────────────────── */
import { useState, useEffect } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
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

function GithubIcon({ size = 16 }: { size?: number }) {
  return (
    <svg
      viewBox="0 0 24 24"
      width={size}
      height={size}
      stroke="currentColor"
      strokeWidth="2"
      fill="none"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22"></path>
    </svg>
  )
}

const APP_ID = 'rfe-auth'

export default function Login() {
  const { login }  = useAuth()
  const [loading, setLoading] = useState(false)
  const [searchParams, setSearchParams] = useSearchParams()
  const returnUrl = searchParams.get('ReturnUrl')
  const errorParam = searchParams.get('error')

  useEffect(() => {
    if (errorParam) {
      toast.error(errorParam)
      const newParams = new URLSearchParams(searchParams)
      newParams.delete('error')
      setSearchParams(newParams, { replace: true })
    }
  }, [errorParam, searchParams, setSearchParams])

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

  function handleGithubLogin() {
    const apiBaseUrl = import.meta.env.VITE_AUTH_API_URL || window.location.origin
    const finalReturn = returnUrl || `${window.location.origin}/`
    window.location.href = `${apiBaseUrl}/api/auth/github/login?role=appUser&redirectUri=${encodeURIComponent(finalReturn)}`
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
        <Button
          type="button"
          fullWidth
          onClick={handleGithubLogin}
          style={{
            background: '#24292e',
            borderColor: '#24292e',
            color: '#ffffff',
            marginTop: '8px',
          }}
        >
          <GithubIcon size={16} />
          <span>Sign in with GitHub</span>
        </Button>
        <p className={styles.footer} style={{ marginTop: '16px' }}>
          Don't have an account? <Link to="/register">Register</Link>
        </p>
        <p className={styles.powered}>Powered by RFE Auth API</p>
      </div>
    </AuthLayout>
  )
}
