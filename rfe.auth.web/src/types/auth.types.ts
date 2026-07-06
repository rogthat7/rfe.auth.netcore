/* ─── Auth Types ──────────────────────────────────────────────────────────── */

export interface LoginRequest {
  phone: string;
  password: string;
  appId: string;
}

export interface RegisterRequest {
  name: string;
  phone: string;
  password: string;
  role: 'Labourer' | 'JobCreator';
  appId: string;
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  expiresAt: string;
}

export interface JwtPayload {
  sub: string;
  name: string;
  phone: string;
  role: string;
  apps: string[];
  exp: number;
  iat: number;
}

export interface CurrentUser {
  id: string;
  name: string;
  phone: string;
  role: string;
  apps: string[];
}
