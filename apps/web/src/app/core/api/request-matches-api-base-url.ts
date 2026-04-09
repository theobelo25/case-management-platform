/**
 * Whether an HttpClient request URL targets the API configured at build time.
 * Empty base means same-origin deployment: relative URLs, or absolute URLs on the current origin.
 * Path-only bases (e.g. `/api`) are used with the dev proxy so requests stay same-origin.
 */
export function requestMatchesApiBaseUrl(requestUrl: string, apiBaseUrl: string): boolean {
  const base = apiBaseUrl.trim();
  if (!base || base === '/') {
    if (!/^https?:\/\//i.test(requestUrl)) {
      return true;
    }
    if (typeof globalThis !== 'undefined' && 'location' in globalThis) {
      const loc = (globalThis as unknown as { location?: { origin: string } }).location;
      if (loc?.origin) {
        try {
          return new URL(requestUrl).origin === loc.origin;
        } catch {
          return false;
        }
      }
    }
    return false;
  }

  // Absolute base URL (e.g. http://localhost:5082)
  if (/^https?:\/\//i.test(base)) {
    return requestUrl.startsWith(base);
  }

  // Path-only base (e.g. /api) — HttpClient resolves to full URL on the current origin
  const normalizedBase = base.replace(/\/+$/, '');
  if (!normalizedBase.startsWith('/')) {
    return false;
  }

  if (!/^https?:\/\//i.test(requestUrl)) {
    return requestUrl === normalizedBase || requestUrl.startsWith(`${normalizedBase}/`);
  }

  try {
    const pathname = new URL(requestUrl).pathname;
    return pathname === normalizedBase || pathname.startsWith(`${normalizedBase}/`);
  } catch {
    return false;
  }
}
