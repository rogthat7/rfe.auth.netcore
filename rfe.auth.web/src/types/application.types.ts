/* ─── Application Types ───────────────────────────────────────────────────── */

import type { UserRole } from './user.types'

export interface Application {
  id: string;
  appId: string;          // e.g. "rfe-glam"
  displayName: string;    // e.g. "Glam Community Platform"
  description?: string;
  allowedRoles: UserRole[];
  userCount: number;
  activeSessionCount: number;
  status: 'Active' | 'Inactive';
  webhookUrl?: string;
  createdAt: string;
  lastActiveAt?: string;
}

export interface CreateApplicationRequest {
  appId: string;
  displayName: string;
  description?: string;
  allowedRoles: UserRole[];
  webhookUrl?: string;
}

export interface AppSwitcherItem {
  appId: string;
  displayName: string;
  userCount: number;
}
