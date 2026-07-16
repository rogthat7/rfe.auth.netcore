/* ─── Register Page ───────────────────────────────────────────────────────── */
import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { User, Mail } from 'lucide-react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'

import { AuthLayout } from '../../../components/layout/AuthLayout/AuthLayout'
import { Button } from '../../../components/ui/Button/Button'
import { Input, PhoneInput, PasswordInput } from '../../../components/ui/Input/Input'
import { useAuth } from '../../../hooks/useAuth'
import { registerSchema, type RegisterFormValues } from '../../../utils/validators'
import { applicationService } from '../../../services/application.service'
import type { Application } from '../../../types/application.types'
import styles from './Register.module.css'

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

const ALLOWED_ROLES = [
  { value: 'Labourer', label: 'Labourer' },
  { value: 'JobCreator', label: 'Job Creator' },
] as const

export default function Register() {
  const { register: registerUser } = useAuth()
  const [loading, setLoading] = useState(false)
  const [apps, setApps] = useState<Application[]>([])

  const { control, register, handleSubmit, watch, formState: { errors } } =
    useForm<RegisterFormValues>({
      resolver: zodResolver(registerSchema),
      defaultValues: { userType: 'App' }
    })

  const userType = watch('userType')

  function handleGithubLogin() {
    const apiBaseUrl = import.meta.env.VITE_AUTH_API_URL || window.location.origin
    const selectedRole = userType === 'Admin' ? 'Admin' : 'appUser'
    window.location.href = `${apiBaseUrl}/api/auth/github/login?role=${selectedRole}&redirectUri=${encodeURIComponent(window.location.origin + '/')}`
  }

  useEffect(() => {
    async function loadApps() {
      try {
        const list = await applicationService.getApplications()
        setApps(list)
      } catch (err) {
        console.error('Failed to load applications', err)
      }
    }
    loadApps()
  }, [])

  async function onSubmit(values: RegisterFormValues) {
    setLoading(true)
    try {
      const selectedAppId = values.userType === 'Admin' ? 'rfe-auth' : values.appId!
      const selectedRole = values.userType === 'Admin' ? 'Admin' : 'appUser'

      await registerUser({
        username: values.username || undefined,
        name: values.name,
        phone: values.phone || undefined,
        email: values.email || undefined,
        password: values.password,
        role: selectedRole,
        appId: selectedAppId,
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
            label="Username (Optional)"
            id="username"
            placeholder="ravikumar"
            icon={<User size={16} />}
            error={errors.username?.message}
            {...register('username')}
          />
          <Input
            label="Full Name"
            id="name"
            placeholder="Ravi Kumar"
            icon={<User size={16} />}
            error={errors.name?.message}
            {...register('name')}
          />
          <Input
            label="Email Address"
            id="email"
            placeholder="ravi@example.com"
            icon={<Mail size={16} />}
            error={errors.email?.message}
            {...register('email')}
          />
          <Controller
            name="phone"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <PhoneInput
                label="Phone Number"
                id="phone"
                value={field.value ?? ''}
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
            <label htmlFor="userType" className={styles.label}>Register As</label>
            <select
              id="userType"
              className={[styles.select, errors.userType ? styles.selectError : ''].join(' ')}
              {...register('userType')}
            >
              <option value="App">App User</option>
              <option value="Admin">Admin</option>
            </select>
            {errors.userType && <span className={styles.error}>{String(errors.userType.message)}</span>}
          </div>

          {userType === 'App' && (
            <>
              <div className={styles.selectWrap}>
                <label htmlFor="appId" className={styles.label}>Select Application</label>
                <select
                  id="appId"
                  className={[styles.select, errors.appId ? styles.selectError : ''].join(' ')}
                  {...register('appId')}
                >
                  <option value="">Choose application…</option>
                  {apps.map((app) => (
                    <option key={app.appId} value={app.appId}>{app.displayName || app.appId}</option>
                  ))}
                </select>
                {errors.appId && <span className={styles.error}>{String(errors.appId.message)}</span>}
              </div>
            </>
          )}

          <Button type="submit" fullWidth loading={loading}>Create Account</Button>
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
          <span>Sign up with GitHub</span>
        </Button>
        <p className={styles.footer}>
          Already have an account? <Link to="/login">Sign In</Link>
        </p>
        <p className={styles.powered}>Powered by RFE Auth API</p>
      </div>
    </AuthLayout>
  )
}
