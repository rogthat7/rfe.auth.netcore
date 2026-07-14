/* ─── Zod Validation Schemas ──────────────────────────────────────────────── */
import { z } from 'zod'

const phoneSchema = z
  .string()
  .min(1, 'Phone number is required')
  .transform((v) => v.replace(/\s/g, ''))
  .refine((v) => /^\d{10}$/.test(v), 'Phone must be exactly 10 digits')

const usernameOrPhoneSchema = z
  .string()
  .min(1, 'Username, email or phone number is required')
  .transform((v) => v.trim())
  .refine((v) => 
    /^\d{10}$/.test(v) || 
    /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) || 
    /^[a-zA-Z0-9_@.-]{3,50}$/.test(v), 
    'Must be a 10-digit phone number, a valid email, or a valid username (3-50 characters)'
  )

export const loginSchema = z.object({
  phone:    usernameOrPhoneSchema,
  password: z.string().min(6, 'Password must be at least 6 characters'),
})
export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z
  .object({
    username:        z.string().min(3, 'Username must be at least 3 characters').regex(/^[a-zA-Z0-9_-]+$/, 'Username can only contain letters, numbers, underscores, and hyphens').optional().or(z.literal('')),
    name:            z.string().min(2, 'Name must be at least 2 characters'),
    phone:           z.string().transform((v) => v ? v.replace(/\s/g, '') : '').refine((v) => v === '' || /^\d{10}$/.test(v), 'Phone must be exactly 10 digits').optional(),
    email:           z.string().refine((v) => !v || v === '' || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v), 'Invalid email address').optional(),
    password:        z.string().min(6, 'Password must be at least 6 characters'),
    confirmPassword: z.string(),
    userType:        z.enum(['Admin', 'App']),
    appId:           z.string().optional(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path:    ['confirmPassword'],
  })
  .refine((d) => (d.phone && d.phone.trim() !== '') || (d.email && d.email.trim() !== ''), {
    message: 'At least one of Phone or Email is required',
    path:    ['phone'],
  })
  .refine((d) => d.userType !== 'App' || (d.appId && d.appId.trim() !== ''), {
    message: 'Please select an application',
    path:    ['appId'],
  })
export type RegisterFormValues = z.infer<typeof registerSchema>

export const createAppSchema = z.object({
  appId:       z.string().min(3, 'App ID must be at least 3 characters')
                .regex(/^[a-z0-9-]+$/, 'App ID must be lowercase letters, numbers, and hyphens'),
  displayName: z.string().min(2, 'Display name is required'),
  description: z.string().optional(),
  allowedRoles: z.array(z.enum(['Admin', 'PanchayatAdmin', 'Labourer', 'JobCreator']))
                 .min(1, 'Select at least one role'),
  webhookUrl:  z.string().url('Must be a valid URL').optional().or(z.literal('')),
})
export type CreateAppFormValues = z.infer<typeof createAppSchema>
