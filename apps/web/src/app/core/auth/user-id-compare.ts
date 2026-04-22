/**
 * Canonical form for comparing user ids across JWT `sub`, `/auth/me`, and org payloads
 * (case and separator differences).
 */
export function normalizeUserId(id: string | null | undefined): string {
  if (id == null || typeof id !== 'string') {
    return '';
  }
  return id.trim().replace(/[{}]/g, '').replace(/-/g, '').toLowerCase();
}
