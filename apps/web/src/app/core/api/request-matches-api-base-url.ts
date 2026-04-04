/**
 * Whether an HttpClient request URL targets the API configured at build time.
 * Empty base means same-origin deployment: relative URLs, or absolute URLs on the current origin.
 */
export function requestMatchesApiBaseUrl(requestUrl: string, apiBaseUrl: string): boolean {
  const base = apiBaseUrl.trim();
  if (base) {
    return requestUrl.startsWith(base);
  }
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
