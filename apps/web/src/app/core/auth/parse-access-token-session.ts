import type { AuthResponseDto } from './auth-api.service';

export type SessionClaims = Omit<AuthResponseDto, 'accessToken'>;

function base64UrlToJson(segment: string): Record<string, unknown> | null {
  try {
    let base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
    const pad = base64.length % 4;
    if (pad) {
      base64 += '='.repeat(4 - pad);
    }
    const json = globalThis.atob(base64);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function parseSessionFromAccessToken(accessToken: string): SessionClaims | null {
  const parts = accessToken.split('.');
  if (parts.length < 2) {
    return null;
  }
  const payload = base64UrlToJson(parts[1]!);
  if (!payload || typeof payload['sub'] !== 'string') {
    return null;
  }

  const exp = payload['exp'];
  const expiresAtUtc =
    typeof exp === 'number' ? new Date(exp * 1000).toISOString() : new Date().toISOString();

  const email = typeof payload['email'] === 'string' ? payload['email'] : '';
  const firstName = typeof payload['given_name'] === 'string' ? payload['given_name'] : '';
  const lastName = typeof payload['family_name'] === 'string' ? payload['family_name'] : '';

  return {
    userId: payload['sub'],
    email,
    firstName,
    lastName,
    expiresAtUtc,
  };
}

export function authUserDisplayName(s: Pick<AuthResponseDto, 'firstName' | 'lastName'>): string {
  return `${s.firstName} ${s.lastName}`.replace(/\s+/g, ' ').trim();
}
