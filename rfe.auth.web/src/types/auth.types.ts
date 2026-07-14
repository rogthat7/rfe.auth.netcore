/* ─── Auth Types ──────────────────────────────────────────────────────────── */

export interface LoginRequest {
  username?: string;
  phone?: string;
  email?: string;
  password: string;
  appId?: string;
}

export interface RegisterRequest {
  username?: string;
  name?: string;
  phone?: string;
  email?: string;
  password: string;
  role: string;
  appId: string;
}

/** Returned by /api/auth/register — signals which verification to do next */
export interface VerificationPending {
  verificationMethod: 'phone' | 'email';
  tokenPayload?: string;
  devOtp?: string;       // only present when SMS not configured
  message: string;
}

export interface VerifyPhoneRequest {
  tokenPayload: string;
  code: string;
}

export interface ResendVerificationRequest {
  identifier: string;
  method: 'phone' | 'email';
  password?: string;
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
