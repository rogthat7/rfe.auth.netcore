/* ─── Role Service ─────────────────────────────────────────────────────────── */
// Swap the mock data below for real API calls when the backend endpoint lands:
//   api.get<ApiResponse<Role[]>>(`/api/roles?appId=${appId}`)
//   api.post<ApiResponse<Role>>('/api/roles', req)
//   api.patch<ApiResponse<Role>>(`/api/roles/${roleId}/permissions`, { permissionId })
//   api.delete(`/api/roles/${roleId}`)

import type { Role, CreateRoleRequest, AddPermissionRequest } from '../types/role.types'

/* ── Seed data – reflects real role/perm model ──────────────────────────── */
const SEED: Role[] = [
  // ── rfe-auth roles ────────────────────────────────────────────────────────────
  {
    id: 'role-admin-rfe-auth',
    name: 'Admin',
    appId: 'rfe-auth',
    color: 'orange',
    description: 'Full system administration. Cannot be self-registered.',
    userCount: 2,
    permissions: [
      { id: 'users.view',        name: 'View Users',         description: 'View the full user list',           category: 'Users'        },
      { id: 'users.create',      name: 'Create User',        description: 'Create new user accounts',          category: 'Users'        },
      { id: 'users.edit',        name: 'Edit User',          description: 'Update user profile & role',        category: 'Users'        },
      { id: 'users.delete',      name: 'Delete User',        description: 'Permanently remove a user account', category: 'Users'        },
      { id: 'roles.view',        name: 'View Roles',         description: 'View role definitions',             category: 'Roles'        },
      { id: 'roles.manage',      name: 'Manage Roles',       description: 'Create, edit, delete roles',        category: 'Roles'        },
      { id: 'apps.view',         name: 'View Applications',  description: 'View registered applications',      category: 'Applications' },
      { id: 'apps.manage',       name: 'Manage Applications', description: 'Register and delete applications', category: 'Applications' },
      { id: 'reports.view',      name: 'View Reports',       description: 'Access analytics and reports',      category: 'Reports'      },
      { id: 'tokens.issue',      name: 'Issue Tokens',       description: 'Generate and issue OAuth tokens',   category: 'Auth'         },
      { id: 'tokens.revoke',     name: 'Revoke Tokens',      description: 'Revoke active OAuth tokens',        category: 'Auth'         },
    ],
  },
  {
    id: 'role-appuser-rfe-auth',
    name: 'app-user',
    appId: 'rfe-auth',
    color: 'indigo',
    description: 'Machine identity for a registered application to call rfe-auth APIs.',
    userCount: 4,
    permissions: [
      { id: 'tokens.issue',      name: 'Issue Tokens',       description: 'Obtain OAuth access tokens via client credentials', category: 'Auth' },
      { id: 'users.lookup',      name: 'Lookup User',        description: 'Resolve a user by ID or token claim',               category: 'Users' },
      { id: 'apps.self.read',    name: 'Read Own App',       description: 'Read own application registration details',          category: 'Applications' },
    ],
  },
  {
    id: 'role-authuser-rfe-auth',
    name: 'auth-user',
    appId: 'rfe-auth',
    color: 'blue',
    description: 'End-user account on rfe-auth. Can query their own profile and session info.',
    userCount: 18,
    permissions: [
      { id: 'profile.read',      name: 'Read Profile',       description: 'Read own user profile',             category: 'Profile'      },
      { id: 'profile.edit',      name: 'Edit Profile',       description: 'Update own profile fields',         category: 'Profile'      },
      { id: 'sessions.view',     name: 'View Sessions',      description: 'List own active sessions',           category: 'Auth'         },
      { id: 'sessions.revoke',   name: 'Revoke Session',     description: 'Revoke own active sessions',        category: 'Auth'         },
    ],
  },
  // ── rfe-glam roles ───────────────────────────────────────────────────────────
  {
    id: 'role-panchayatadmin-rfe-glam',
    name: 'PanchayatAdmin',
    appId: 'rfe-glam',
    color: 'cyan',
    description: 'Panchayat-level admin. Manages community data, approvals, and postings.',
    userCount: 8,
    permissions: [
      { id: 'users.view',        name: 'View Users',         description: 'View community user list',          category: 'Users'        },
      { id: 'users.edit',        name: 'Edit User',          description: 'Update user profile & role',        category: 'Users'        },
      { id: 'jobs.view',         name: 'View Jobs',          description: 'Browse all job listings',           category: 'Jobs'         },
      { id: 'jobs.approve',      name: 'Approve Jobs',       description: 'Approve or reject job postings',    category: 'Jobs'         },
      { id: 'reports.view',      name: 'View Reports',       description: 'Access community analytics',        category: 'Reports'      },
    ],
  },
  {
    id: 'role-jobcreator-rfe-glam',
    name: 'JobCreator',
    appId: 'rfe-glam',
    color: 'indigo',
    description: 'App user. Can post job listings and hire labourers.',
    userCount: 52,
    permissions: [
      { id: 'jobs.view',         name: 'View Jobs',          description: 'Browse available job listings',     category: 'Jobs'         },
      { id: 'jobs.create',       name: 'Post Jobs',          description: 'Create and publish job listings',   category: 'Jobs'         },
      { id: 'jobs.manage',       name: 'Manage Jobs',        description: 'Edit and close own job listings',   category: 'Jobs'         },
      { id: 'profile.read',      name: 'Read Profile',       description: 'Read own profile',                  category: 'Profile'      },
    ],
  },
  {
    id: 'role-labourer-rfe-glam',
    name: 'Labourer',
    appId: 'rfe-glam',
    color: 'blue',
    description: 'App user. Can browse jobs and submit applications.',
    userCount: 340,
    permissions: [
      { id: 'jobs.view',         name: 'View Jobs',          description: 'Browse available job listings',     category: 'Jobs'         },
      { id: 'jobs.apply',        name: 'Apply to Jobs',      description: 'Submit job applications',           category: 'Jobs'         },
      { id: 'profile.read',      name: 'Read Profile',       description: 'Read own profile',                  category: 'Profile'      },
      { id: 'profile.edit',      name: 'Edit Profile',       description: 'Update own profile fields',         category: 'Profile'      },
    ],
  },
  // ── rfe-admin roles ───────────────────────────────────────────────────────────
  {
    id: 'role-admin-rfe-admin',
    name: 'Admin',
    appId: 'rfe-admin',
    color: 'orange',
    description: 'Full administrative access to admin console.',
    userCount: 3,
    permissions: [
      { id: 'users.view',        name: 'View Users',         description: 'View the full user list',           category: 'Users'        },
      { id: 'users.create',      name: 'Create User',        description: 'Create new user accounts',          category: 'Users'        },
      { id: 'users.edit',        name: 'Edit User',          description: 'Update user profile & role',        category: 'Users'        },
      { id: 'users.delete',      name: 'Delete User',        description: 'Permanently remove a user account', category: 'Users'        },
      { id: 'apps.view',         name: 'View Applications',  description: 'View registered applications',      category: 'Applications' },
      { id: 'apps.manage',       name: 'Manage Applications', description: 'Register and delete applications', category: 'Applications' },
      { id: 'reports.view',      name: 'View Reports',       description: 'Access analytics and reports',      category: 'Reports'      },
      { id: 'reports.export',    name: 'Export Reports',     description: 'Download and export report data',   category: 'Reports'      },
    ],
  },
]

/* In-memory store (replace with API calls when backend is ready) */
let _store: Role[] = SEED.map((r) => ({ ...r, permissions: [...r.permissions] }))

let _idCounter = 100

export const roleService = {
  async getRolesByApp(appId: string): Promise<Role[]> {
    return _store.filter((r) => r.appId === appId)
  },

  async createRole(req: CreateRoleRequest): Promise<Role> {
    const role: Role = {
      id: `role-${req.name.toLowerCase().replace(/\s+/g, '-')}-${req.appId}-${++_idCounter}`,
      name: req.name,
      appId: req.appId,
      color: req.color,
      description: req.description ?? '',
      userCount: 0,
      permissions: [],
    }
    _store = [..._store, role]
    return role
  },

  async deleteRole(roleId: string): Promise<void> {
    _store = _store.filter((r) => r.id !== roleId)
  },

  async addPermission(req: AddPermissionRequest): Promise<Role> {
    _store = _store.map((r) => {
      if (r.id !== req.roleId) return r
      if (r.permissions.some((p) => p.id === req.permissionId)) return r
      return {
        ...r,
        permissions: [
          ...r.permissions,
          { id: req.permissionId, name: req.name, description: req.description, category: req.category },
        ],
      }
    })
    return _store.find((r) => r.id === req.roleId)!
  },

  async removePermission(roleId: string, permissionId: string): Promise<Role> {
    _store = _store.map((r) => {
      if (r.id !== roleId) return r
      return { ...r, permissions: r.permissions.filter((p) => p.id !== permissionId) }
    })
    return _store.find((r) => r.id === roleId)!
  },
}
