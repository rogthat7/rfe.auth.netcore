/* ─── User Types ──────────────────────────────────────────────────────────── */

export type UserRole = 'Admin' | 'PanchayatAdmin' | 'Labourer' | 'JobCreator';
export type UserStatus = 'Active' | 'Inactive';

export interface User {
  id: string;
  name: string;
  phone: string;
  role: UserRole;
  apps: string[];
  status: UserStatus;
  createdAt: string;
  lastLoginAt?: string;
}

export interface CreateUserRequest {
  name: string;
  phone: string;
  password: string;
  role: UserRole;
  appId: string;
}

export interface UpdateUserRequest {
  name?: string;
  role?: UserRole;
  status?: UserStatus;
  apps?: string[];
}
