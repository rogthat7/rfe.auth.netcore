/* ─── Role & Permission Types ─────────────────────────────────────────────── */

export interface Permission {
  id: string          // e.g. "users.view"
  name: string        // e.g. "View Users"
  description: string // e.g. "Can view the user list"
  category: string    // e.g. "Users"
}

export interface Role {
  id: string
  name: string
  appId: string
  color: 'blue' | 'indigo' | 'orange' | 'cyan' | 'grey'
  description: string
  userCount: number
  permissions: Permission[]
}

export interface CreateRoleRequest {
  name: string
  appId: string
  color: Role['color']
  description?: string
  permissions?: string[]  // permission IDs
}

export interface AddPermissionRequest {
  roleId: string
  appId: string
  permissionId: string
  name: string
  description: string
  category: string
}
