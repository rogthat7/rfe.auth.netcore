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

function GoogleIcon({ size = 16 }: { size?: number }) {
  return (
    <svg
      viewBox="0 0 24 24"
      width={size}
      height={size}
      fill="currentColor"
    >
      <path
        d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
        fill="#4285F4"
      />
      <path
        d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
        fill="#34A853"
      />
      <path
        d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"
        fill="#FBBC05"
      />
      <path
        d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"
        fill="#EA4335"
      />
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
    window.location.href = `${apiBaseUrl}/api/auth/github/login?role=authUser&redirectUri=${encodeURIComponent(finalReturn)}`
  }

  function handleGoogleLogin() {
    const apiBaseUrl = import.meta.env.VITE_AUTH_API_URL || window.location.origin
    const finalReturn = returnUrl || `${window.location.origin}/`
    window.location.href = `${apiBaseUrl}/api/auth/google/login?role=authUser&redirectUri=${encodeURIComponent(finalReturn)}`
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
        <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', marginTop: '8px' }}>
          <Button
            type="button"
            fullWidth
            onClick={handleGithubLogin}
            style={{
              background: '#24292e',
              borderColor: '#24292e',
              color: '#ffffff',
            }}
          >
            <GithubIcon size={16} />
            <span>Sign in with GitHub</span>
          </Button>
          <Button
            type="button"
            fullWidth
            onClick={handleGoogleLogin}
            style={{
              background: '#ffffff',
              borderColor: '#dadce0',
              color: '#3c4043',
            }}
          >
            <GoogleIcon size={16} />
            <span>Sign in with Google</span>
          </Button>
        </div>
        <p className={styles.footer} style={{ marginTop: '16px' }}>
          Don't have an account? <Link to="/register">Register</Link>
        </p>
        <p className={styles.powered}>Powered by RFE Auth API</p>
      </div>
    </AuthLayout>
  )
}
