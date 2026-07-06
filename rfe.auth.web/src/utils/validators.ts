/* ─── Zod Validation Schemas ──────────────────────────────────────────────── */
import { z } from 'zod'

const phoneSchema = z
  .string()
  .min(1, 'Phone number is required')
  .transform((v) => v.replace(/\s/g, ''))
  .refine((v) => /^\d{10}$/.test(v), 'Phone must be exactly 10 digits')

export const loginSchema = z.object({
  phone:    phoneSchema,
  password: z.string().min(6, 'Password must be at least 6 characters'),
})
export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z
  .object({
    name:            z.string().min(2, 'Name must be at least 2 characters'),
    phone:           phoneSchema,
    password:        z.string().min(6, 'Password must be at least 6 characters'),
    confirmPassword: z.string(),
    role:            z.enum(['Labourer', 'JobCreator']).refine((v) => v !== undefined, {
      message: 'Please select a role',
    }),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path:    ['confirmPassword'],
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
