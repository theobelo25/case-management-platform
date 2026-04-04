import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';
import { auth401Interceptor } from './auth-401.interceptor';
import { authInterceptor } from './auth.interceptor';
import { withCredentialsInterceptor } from './refresh.interceptor';

describe('auth401Interceptor', () => {
  const baseUrl = 'http://api.test';
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: AuthService;

  const authBody = {
    accessToken: 't1',
    expiresAtUtc: '2026-01-01T00:00:00Z',
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'u@test.com',
    fullName: 'U',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(
          withInterceptors([
            withCredentialsInterceptor,
            authInterceptor,
            auth401Interceptor,
          ]),
        ),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function signInSession(): void {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/sign-in`).flush(authBody);
  }

  it('on 401, refreshes once and retries the original request with X-Auth-Retry', () => {
    signInSession();

    const refreshed = { ...authBody, accessToken: 't2' };
    let finalPayload: unknown;

    http.get(`${baseUrl}/cases`).subscribe((body) => {
      finalPayload = body;
    });

    const first = httpMock.expectOne(`${baseUrl}/cases`);
    expect(first.request.headers.get('Authorization')).toBe('Bearer t1');
    first.flush(null, { status: 401, statusText: 'Unauthorized' });

    const refresh = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    expect(refresh.request.method).toBe('POST');
    expect(refresh.request.withCredentials).toBe(true);
    refresh.flush(refreshed);

    const retry = httpMock.expectOne(`${baseUrl}/cases`);
    expect(retry.request.headers.get('Authorization')).toBe('Bearer t2');
    expect(retry.request.headers.get('X-Auth-Retry')).toBe('1');
    retry.flush({ ok: true });

    expect(finalPayload).toEqual({ ok: true });
    expect(auth.accessTokenReadonly()).toBe('t2');
  });

  it('does not refresh when the failing request is an auth endpoint', () => {
    signInSession();

    let errorStatus: number | undefined;
    http.post(`${baseUrl}/auth/sign-in`, { email: 'x', password: 'y' }).subscribe({
      error: (e) => {
        errorStatus = e.status;
      },
    });

    const req = httpMock.expectOne(`${baseUrl}/auth/sign-in`);
    req.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(errorStatus).toBe(401);
    httpMock.expectNone(`${baseUrl}/auth/refresh`);
  });

  it('when refresh fails, signs out and propagates the refresh error', () => {
    signInSession();

    let errorStatus: number | undefined;
    http.get(`${baseUrl}/cases`).subscribe({
      error: (e) => {
        errorStatus = e.status;
      },
    });

    httpMock.expectOne(`${baseUrl}/cases`).flush(null, { status: 401, statusText: 'Unauthorized' });

    const refresh = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    refresh.flush(null, { status: 401, statusText: 'Unauthorized' });

    const signOut = httpMock.expectOne(`${baseUrl}/auth/sign-out`);
    signOut.flush(null, { status: 204, statusText: 'No Content' });

    expect(errorStatus).toBe(401);
    expect(auth.accessTokenReadonly()).toBeNull();
  });

  it('does not sign out when the retried request returns 401 (only refresh failure signs out)', () => {
    signInSession();

    http.get(`${baseUrl}/cases`).subscribe({ error: () => undefined });

    httpMock.expectOne(`${baseUrl}/cases`).flush(null, { status: 401, statusText: 'Unauthorized' });
    httpMock.expectOne(`${baseUrl}/auth/refresh`).flush({ ...authBody, accessToken: 't2' });

    const retry = httpMock.expectOne(`${baseUrl}/cases`);
    expect(retry.request.headers.get('X-Auth-Retry')).toBe('1');
    retry.flush(null, { status: 401, statusText: 'Unauthorized' });

    httpMock.expectNone(`${baseUrl}/auth/sign-out`);
    expect(auth.accessTokenReadonly()).toBe('t2');
  });
});
