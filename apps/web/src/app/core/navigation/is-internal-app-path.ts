/** In-app navigation only; blocks protocol-relative and scheme URLs (open redirects). */
export function isInternalAppPath(url: string): boolean {
  return url.startsWith('/') && !url.startsWith('//') && !/^[a-zA-Z][a-zA-Z\d+\-.]*:/.test(url);
}
