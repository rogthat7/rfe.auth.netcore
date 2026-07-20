/* ─── Roles & Permissions Page ─────────────────────────────────────────────── */
import { useState, useEffect, useMemo } from 'react'
import {
  Shield, Plus, Trash2, ChevronRight, Lock, LockOpen,
  AppWindow, Users, X, Check, AlertTriangle,
} from 'lucide-react'
import { Modal }  from '../../components/ui/Modal/Modal'
import { Button } from '../../components/ui/Button/Button'
import { applicationService } from '../../services/application.service'
import { roleService }        from '../../services/role.service'
import type { Application }   from '../../types/application.types'
import type { Role }          from '../../types/role.types'
import { toast } from 'sonner'
import styles from './Roles.module.css'

/* ── Constants ─────────────────────────────────────────────────────────────── */
const COLOR_OPTIONS: Role['color'][] = ['blue', 'indigo', 'orange', 'cyan', 'grey']

const PERMISSION_CATEGORIES = ['Users', 'Roles', 'Applications', 'Jobs', 'Reports', 'Settings']

const PERMISSION_SUGGESTIONS: Record<string, { id: string; name: string; description: string }[]> = {
  Users:        [
    { id: 'users.view',   name: 'View Users',   description: 'Browse the user list' },
    { id: 'users.create', name: 'Create User',  description: 'Create new accounts' },
    { id: 'users.edit',   name: 'Edit User',    description: 'Update profile & role' },
    { id: 'users.delete', name: 'Delete User',  description: 'Remove user accounts' },
  ],
  Roles:        [
    { id: 'roles.view',   name: 'View Roles',   description: 'View role definitions' },
    { id: 'roles.manage', name: 'Manage Roles', description: 'Create, edit, delete roles' },
  ],
  Applications: [
    { id: 'apps.view',    name: 'View Apps',    description: 'View registered applications' },
    { id: 'apps.manage',  name: 'Manage Apps',  description: 'Register and delete apps' },
  ],
  Jobs:         [
    { id: 'jobs.view',    name: 'View Jobs',    description: 'Browse job listings' },
    { id: 'jobs.apply',   name: 'Apply Jobs',   description: 'Submit applications' },
    { id: 'jobs.create',  name: 'Post Jobs',    description: 'Create job listings' },
    { id: 'jobs.manage',  name: 'Manage Jobs',  description: 'Edit and close listings' },
  ],
  Reports:      [
    { id: 'reports.view', name: 'View Reports', description: 'Access analytics' },
    { id: 'reports.export', name: 'Export Reports', description: 'Download report data' },
  ],
  Settings:     [
    { id: 'settings.view',  name: 'View Settings',   description: 'View system settings' },
    { id: 'settings.edit',  name: 'Edit Settings',   description: 'Change system settings' },
  ],
}

/* ── Sub-components ─────────────────────────────────────────────────────────── */

function RoleColorDot({ color }: { color: Role['color'] }) {
  return <span className={[styles.colorDot, styles[`dot_${color}`]].join(' ')} />
}

function PermissionRow({
  permission,
  onRemove,
}: {
  permission: Role['permissions'][number]
  onRemove: (id: string) => void
}) {
  return (
    <div className={styles.permRow}>
      <div className={styles.permIcon}>
        <Lock size={13} />
      </div>
      <div className={styles.permInfo}>
        <span className={styles.permName}>{permission.name}</span>
        <span className={styles.permId}>{permission.id}</span>
        <span className={styles.permDesc}>{permission.description}</span>
      </div>
      <button
        className={styles.removeBtn}
        onClick={() => onRemove(permission.id)}
        title="Remove permission"
        aria-label={`Remove ${permission.name}`}
      >
        <X size={13} />
      </button>
    </div>
  )
}

/* ── Main Page ──────────────────────────────────────────────────────────────── */
export default function Roles() {
  /* App tab state */
  const [apps, setApps]               = useState<Application[]>([])
  const [appsLoading, setAppsLoading] = useState(true)
  const [activeAppId, setActiveAppId] = useState<string>('')

  /* Role list state */
  const [roles, setRoles]           = useState<Role[]>([])
  const [rolesLoading, setRolesLoading] = useState(false)
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null)

  /* Modals */
  const [addRoleOpen, setAddRoleOpen]         = useState(false)
  const [addPermOpen, setAddPermOpen]         = useState(false)
  const [deleteRoleOpen, setDeleteRoleOpen]   = useState(false)

  /* Add Role form */
  const [newRoleName, setNewRoleName]     = useState('')
  const [newRoleDesc, setNewRoleDesc]     = useState('')
  const [newRoleColor, setNewRoleColor]   = useState<Role['color']>('blue')
  const [addingRole, setAddingRole]       = useState(false)

  /* Add Permission form */
  const [newPermCategory, setNewPermCategory] = useState(PERMISSION_CATEGORIES[0])
  const [newPermId, setNewPermId]             = useState('')
  const [newPermName, setNewPermName]         = useState('')
  const [newPermDesc, setNewPermDesc]         = useState('')
  const [addingPerm, setAddingPerm]           = useState(false)

  /* Deleting role */
  const [deletingRole, setDeletingRole] = useState(false)

  /* ── Load apps ── */
  useEffect(() => {
    setAppsLoading(true)
    applicationService.getApplications()
      .then((data) => {
        setApps(data)
        if (data.length > 0) setActiveAppId(data[0].appId)
      })
      .catch(() => toast.error('Failed to load applications'))
      .finally(() => setAppsLoading(false))
  }, [])

  /* ── Load roles when app tab changes ── */
  useEffect(() => {
    if (!activeAppId) return
    setRolesLoading(true)
    setSelectedRoleId(null)
    roleService.getRolesByApp(activeAppId)
      .then((data) => {
        setRoles(data)
        if (data.length > 0) setSelectedRoleId(data[0].id)
      })
      .catch(() => toast.error('Failed to load roles'))
      .finally(() => setRolesLoading(false))
  }, [activeAppId])

  const selectedRole = useMemo(
    () => roles.find((r) => r.id === selectedRoleId) ?? null,
    [roles, selectedRoleId]
  )

  /* Group permissions by category */
  const permsByCategory = useMemo(() => {
    if (!selectedRole) return {}
    return selectedRole.permissions.reduce<Record<string, Role['permissions']>>((acc, p) => {
      ;(acc[p.category] ??= []).push(p)
      return acc
    }, {})
  }, [selectedRole])

  /* ── Handlers ── */
  async function handleAddRole(e: React.FormEvent) {
    e.preventDefault()
    const name = newRoleName.trim()
    if (!name) { toast.error('Role name is required'); return }
    if (!/^[A-Za-z][A-Za-z0-9_\- ]*$/.test(name)) {
      toast.error('Role name must start with a letter and contain only letters, numbers, spaces, hyphens, or underscores')
      return
    }
    setAddingRole(true)
    try {
      const created = await roleService.createRole({
        name,
        appId: activeAppId,
        color: newRoleColor,
        description: newRoleDesc.trim(),
      })
      setRoles((prev) => [...prev, created])
      setSelectedRoleId(created.id)
      setAddRoleOpen(false)
      setNewRoleName('')
      setNewRoleDesc('')
      setNewRoleColor('blue')
      toast.success(`Role "${created.name}" created`)
    } catch {
      toast.error('Failed to create role')
    } finally {
      setAddingRole(false)
    }
  }

  async function handleDeleteRole() {
    if (!selectedRole) return
    setDeletingRole(true)
    try {
      await roleService.deleteRole(selectedRole.id)
      const remaining = roles.filter((r) => r.id !== selectedRole.id)
      setRoles(remaining)
      setSelectedRoleId(remaining[0]?.id ?? null)
      setDeleteRoleOpen(false)
      toast.success(`Role "${selectedRole.name}" deleted`)
    } catch {
      toast.error('Failed to delete role')
    } finally {
      setDeletingRole(false)
    }
  }

  async function handleAddPermission(e: React.FormEvent) {
    e.preventDefault()
    if (!selectedRole) return
    const id   = newPermId.trim()
    const name = newPermName.trim()
    if (!id || !name) { toast.error('Permission ID and name are required'); return }
    if (!/^[a-z][a-z0-9.]*$/.test(id)) {
      toast.error('Permission ID must be lowercase letters, numbers, and dots only (e.g. users.view)')
      return
    }
    setAddingPerm(true)
    try {
      const updated = await roleService.addPermission({
        roleId: selectedRole.id,
        appId: activeAppId,
        permissionId: id,
        name,
        description: newPermDesc.trim(),
        category: newPermCategory,
      })
      setRoles((prev) => prev.map((r) => (r.id === updated.id ? updated : r)))
      setAddPermOpen(false)
      setNewPermId('')
      setNewPermName('')
      setNewPermDesc('')
      setNewPermCategory(PERMISSION_CATEGORIES[0])
      toast.success(`Permission "${name}" added`)
    } catch {
      toast.error('Failed to add permission')
    } finally {
      setAddingPerm(false)
    }
  }

  async function handleRemovePermission(permId: string) {
    if (!selectedRole) return
    try {
      const updated = await roleService.removePermission(selectedRole.id, permId)
      setRoles((prev) => prev.map((r) => (r.id === updated.id ? updated : r)))
      toast.success('Permission removed')
    } catch {
      toast.error('Failed to remove permission')
    }
  }

  /* Fill suggestion fields when category + suggestion changes */
  function applySuggestion(s: { id: string; name: string; description: string }) {
    setNewPermId(s.id)
    setNewPermName(s.name)
    setNewPermDesc(s.description)
  }

  /* ── Render ── */
  return (
    <div className={styles.page}>

      {/* ── Page Header ── */}
      <div className={styles.header}>
        <div>
          <h2 className={styles.title}>Roles &amp; Permissions</h2>
          <p className={styles.subtitle}>
            Manage per-application role definitions and permission assignments
          </p>
        </div>
      </div>

      {/* ── App Tab Strip ── */}
      <div className={styles.tabStrip}>
        {appsLoading ? (
          <span className={styles.tabLoading}>Loading apps…</span>
        ) : (
          apps.map((app) => (
            <button
              key={app.appId}
              id={`app-tab-${app.appId}`}
              className={[styles.tab, app.appId === activeAppId ? styles.tabActive : ''].join(' ')}
              onClick={() => setActiveAppId(app.appId)}
            >
              <AppWindow size={14} />
              {app.displayName || app.appId}
              <span className={[
                styles.tabDot,
                app.status === 'Active' ? styles.tabDotActive : styles.tabDotInactive,
              ].join(' ')} />
            </button>
          ))
        )}
      </div>

      {/* ── Two-column body ── */}
      <div className={styles.body}>

        {/* ── Roles Column ── */}
        <aside className={styles.rolesCol}>
          <div className={styles.colHeader}>
            <span className={styles.colTitle}>
              <Shield size={15} /> Roles
            </span>
            <button
              id="add-role-btn"
              className={styles.addRoleBtn}
              onClick={() => setAddRoleOpen(true)}
              title="Add role"
              aria-label="Add new role"
            >
              <Plus size={15} />
            </button>
          </div>

          {rolesLoading ? (
            <div className={styles.rolesLoading}>Loading roles…</div>
          ) : roles.length === 0 ? (
            <div className={styles.emptyRoles}>
              <Shield size={32} className={styles.emptyIcon} />
              <p>No roles for this app yet.</p>
              <Button size="sm" onClick={() => setAddRoleOpen(true)}>
                <Plus size={14} /> Add Role
              </Button>
            </div>
          ) : (
            <ul className={styles.roleList} role="listbox" aria-label="Roles">
              {roles.map((role) => (
                <li
                  key={role.id}
                  id={`role-item-${role.id}`}
                  role="option"
                  aria-selected={role.id === selectedRoleId}
                  className={[
                    styles.roleItem,
                    role.id === selectedRoleId ? styles.roleItemActive : '',
                  ].join(' ')}
                  onClick={() => setSelectedRoleId(role.id)}
                >
                  <RoleColorDot color={role.color} />
                  <div className={styles.roleItemContent}>
                    <span className={styles.roleItemName}>{role.name}</span>
                    <span className={styles.roleItemMeta}>
                      <Users size={11} /> {role.userCount.toLocaleString()} users
                      &nbsp;·&nbsp;
                      <Lock size={11} /> {role.permissions.length} perms
                    </span>
                  </div>
                  <ChevronRight size={14} className={styles.roleChevron} />
                </li>
              ))}
            </ul>
          )}
        </aside>

        {/* ── Permissions Panel ── */}
        <section className={styles.permsPanel}>
          {!selectedRole ? (
            <div className={styles.emptyPanel}>
              <LockOpen size={40} className={styles.emptyPanelIcon} />
              <p>Select a role to manage its permissions.</p>
            </div>
          ) : (
            <>
              {/* Panel header */}
              <div className={styles.panelHeader}>
                <div className={styles.panelTitle}>
                  <RoleColorDot color={selectedRole.color} />
                  <span className={styles.panelRoleName}>{selectedRole.name}</span>
                  <span className={styles.panelAppBadge}>{activeAppId}</span>
                </div>
                <p className={styles.panelDesc}>{selectedRole.description}</p>
                <div className={styles.panelMeta}>
                  <span className={styles.metaChip}>
                    <Users size={12} /> {selectedRole.userCount.toLocaleString()} users
                  </span>
                  <span className={styles.metaChip}>
                    <Lock size={12} /> {selectedRole.permissions.length} permissions
                  </span>
                </div>
              </div>

              {/* Permission categories */}
              <div className={styles.permCategories}>
                {Object.keys(permsByCategory).length === 0 ? (
                  <div className={styles.noPerms}>
                    <LockOpen size={24} />
                    <span>No permissions assigned — add one below.</span>
                  </div>
                ) : (
                  Object.entries(permsByCategory).map(([cat, perms]) => (
                    <div key={cat} className={styles.permCategory}>
                      <h4 className={styles.categoryTitle}>{cat}</h4>
                      <div className={styles.permRows}>
                        {perms.map((p) => (
                          <PermissionRow
                            key={p.id}
                            permission={p}
                            onRemove={handleRemovePermission}
                          />
                        ))}
                      </div>
                    </div>
                  ))
                )}
              </div>

              {/* Panel actions */}
              <div className={styles.panelActions}>
                <Button
                  id="add-permission-btn"
                  size="sm"
                  onClick={() => setAddPermOpen(true)}
                >
                  <Plus size={14} /> Add Permission
                </Button>
                <Button
                  id="delete-role-btn"
                  size="sm"
                  variant="secondary"
                  onClick={() => setDeleteRoleOpen(true)}
                  className={styles.deleteRoleBtn}
                >
                  <Trash2 size={14} /> Delete Role
                </Button>
              </div>
            </>
          )}
        </section>
      </div>

      {/* ── Add Role Modal ── */}
      <Modal open={addRoleOpen} onClose={() => setAddRoleOpen(false)} title="Add New Role" width={440}>
        <form className={styles.modalForm} onSubmit={handleAddRole}>
          <div className={styles.field}>
            <label htmlFor="new-role-name">Role Name</label>
            <input
              id="new-role-name"
              className={styles.input}
              placeholder="e.g. Moderator"
              value={newRoleName}
              onChange={(e) => setNewRoleName(e.target.value)}
              autoFocus
              required
            />
            <span className={styles.hint}>Letters, numbers, spaces, hyphens, underscores</span>
          </div>
          <div className={styles.field}>
            <label htmlFor="new-role-desc">Description</label>
            <textarea
              id="new-role-desc"
              className={styles.textarea}
              rows={2}
              placeholder="Brief description of this role…"
              value={newRoleDesc}
              onChange={(e) => setNewRoleDesc(e.target.value)}
            />
          </div>
          <div className={styles.field}>
            <label>Color</label>
            <div className={styles.colorPicker}>
              {COLOR_OPTIONS.map((c) => (
                <button
                  key={c}
                  type="button"
                  id={`color-${c}`}
                  className={[
                    styles.colorOption,
                    styles[`colorOpt_${c}`],
                    newRoleColor === c ? styles.colorOptSelected : '',
                  ].join(' ')}
                  onClick={() => setNewRoleColor(c)}
                  aria-label={`Color: ${c}`}
                >
                  {newRoleColor === c && <Check size={12} />}
                </button>
              ))}
            </div>
          </div>
          <div className={styles.modalActions}>
            <Button variant="secondary" type="button" onClick={() => setAddRoleOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" loading={addingRole}>
              Create Role
            </Button>
          </div>
        </form>
      </Modal>

      {/* ── Add Permission Modal ── */}
      <Modal open={addPermOpen} onClose={() => setAddPermOpen(false)} title="Add Permission" width={500}>
        <form className={styles.modalForm} onSubmit={handleAddPermission}>
          <div className={styles.field}>
            <label htmlFor="perm-category">Category</label>
            <select
              id="perm-category"
              className={styles.select}
              value={newPermCategory}
              onChange={(e) => {
                setNewPermCategory(e.target.value)
                setNewPermId('')
                setNewPermName('')
                setNewPermDesc('')
              }}
            >
              {PERMISSION_CATEGORIES.map((c) => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>
          </div>

          {/* Quick-pick suggestions */}
          {PERMISSION_SUGGESTIONS[newPermCategory]?.length > 0 && (
            <div className={styles.field}>
              <label>Quick Pick</label>
              <div className={styles.suggestions}>
                {PERMISSION_SUGGESTIONS[newPermCategory]
                  .filter((s) => !selectedRole?.permissions.some((p) => p.id === s.id))
                  .map((s) => (
                    <button
                      key={s.id}
                      type="button"
                      id={`suggestion-${s.id}`}
                      className={[
                        styles.suggestion,
                        newPermId === s.id ? styles.suggestionActive : '',
                      ].join(' ')}
                      onClick={() => applySuggestion(s)}
                    >
                      <span className={styles.suggestionId}>{s.id}</span>
                      <span className={styles.suggestionName}>{s.name}</span>
                    </button>
                  ))}
              </div>
            </div>
          )}

          <div className={styles.field}>
            <label htmlFor="perm-id">Permission ID</label>
            <input
              id="perm-id"
              className={styles.input}
              placeholder="e.g. users.view"
              value={newPermId}
              onChange={(e) => setNewPermId(e.target.value)}
              pattern="^[a-z][a-z0-9.]*$"
              title="Lowercase letters, numbers, dots only"
              required
            />
            <span className={styles.hint}>Lowercase, dots as separators (e.g. users.view)</span>
          </div>
          <div className={styles.field}>
            <label htmlFor="perm-name">Display Name</label>
            <input
              id="perm-name"
              className={styles.input}
              placeholder="e.g. View Users"
              value={newPermName}
              onChange={(e) => setNewPermName(e.target.value)}
              required
            />
          </div>
          <div className={styles.field}>
            <label htmlFor="perm-desc">Description</label>
            <input
              id="perm-desc"
              className={styles.input}
              placeholder="What this permission allows…"
              value={newPermDesc}
              onChange={(e) => setNewPermDesc(e.target.value)}
            />
          </div>
          <div className={styles.modalActions}>
            <Button variant="secondary" type="button" onClick={() => setAddPermOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" loading={addingPerm}>
              Add Permission
            </Button>
          </div>
        </form>
      </Modal>

      {/* ── Delete Role Confirm Modal ── */}
      <Modal
        open={deleteRoleOpen}
        onClose={() => setDeleteRoleOpen(false)}
        title="Delete Role"
        width={400}
      >
        <div className={styles.confirmBody}>
          <div className={styles.confirmIcon}>
            <AlertTriangle size={28} />
          </div>
          <p className={styles.confirmText}>
            Are you sure you want to delete the role{' '}
            <strong>"{selectedRole?.name}"</strong>?
          </p>
          <p className={styles.confirmSub}>
            This will remove the role from <strong>{activeAppId}</strong>. Users currently
            assigned this role will lose access. This action cannot be undone.
          </p>
          <div className={styles.modalActions}>
            <Button variant="secondary" type="button" onClick={() => setDeleteRoleOpen(false)}>
              Cancel
            </Button>
            <Button
              id="confirm-delete-role-btn"
              type="button"
              loading={deletingRole}
              onClick={handleDeleteRole}
              className={styles.dangerBtn}
            >
              <Trash2 size={14} /> Delete Role
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
