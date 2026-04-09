import { afterEach, describe, expect, it, vi } from 'vitest';
import { requestMatchesApiBaseUrl } from './request-matches-api-base-url';

describe('requestMatchesApiBaseUrl', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('returns true when request URL starts with configured API base', () => {
    expect(
      requestMatchesApiBaseUrl('http://localhost:5082/api/v1/cases', 'http://localhost:5082'),
    ).toBe(true);
  });

  it('returns false when request URL does not start with API base', () => {
    expect(requestMatchesApiBaseUrl('http://other-host/api', 'http://localhost:5082')).toBe(false);
  });

  it('trims API base before comparing', () => {
    expect(requestMatchesApiBaseUrl('http://x/y', '  http://x  ')).toBe(true);
  });

  describe('path-only API base (dev proxy, e.g. /api)', () => {
    it('matches absolute URLs whose pathname is under the base', () => {
      expect(
        requestMatchesApiBaseUrl('http://localhost:4200/api/cases', '/api'),
      ).toBe(true);
      expect(requestMatchesApiBaseUrl('http://localhost:4200/api/auth/refresh', '/api')).toBe(
        true,
      );
      expect(requestMatchesApiBaseUrl('http://localhost:4200/app/cases', '/api')).toBe(false);
    });

    it('matches relative URLs under the base', () => {
      expect(requestMatchesApiBaseUrl('/api/cases', '/api')).toBe(true);
      expect(requestMatchesApiBaseUrl('/app/cases', '/api')).toBe(false);
    });

    it('trims trailing slashes on the configured base', () => {
      expect(requestMatchesApiBaseUrl('http://localhost:4200/api/cases', '/api/')).toBe(true);
    });
  });

  describe('empty API base (same-origin deployment)', () => {
    it('treats relative URLs as API requests', () => {
      expect(requestMatchesApiBaseUrl('/api/health', '')).toBe(true);
    });

    it('compares http(s) absolute URLs to the current origin', () => {
      vi.stubGlobal('location', { origin: 'https://app.example.com' });
      expect(requestMatchesApiBaseUrl('https://app.example.com/api/x', '')).toBe(true);
      expect(requestMatchesApiBaseUrl('https://evil.example/api/x', '')).toBe(false);
    });

    it('returns false for malformed absolute URLs when location is available', () => {
      vi.stubGlobal('location', { origin: 'https://app.example.com' });
      expect(requestMatchesApiBaseUrl('https://%ZZ/api', '')).toBe(false);
    });
  });
});
