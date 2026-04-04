import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  const baseUrl = 'http://api.test';
  let auth: AuthService;
  let httpMock: HttpTestingController;

  const authBody = {
    accessToken: 'token-1',
    expiresAtUtc: '2026-01-01T00:00:00Z',
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    fullName: 'Test User',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    auth = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('signIn stores access token and session from the API response', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/sign-in`);
    req.flush(authBody);

    expect(auth.accessTokenReadonly()).toBe('token-1');
    expect(auth.session()).toEqual(authBody);
  });

  it('refreshSession replaces token and session', () => {
    const refreshed = { ...authBody, accessToken: 'token-2' };

    auth.refreshSession().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    req.flush(refreshed);

    expect(auth.accessTokenReadonly()).toBe('token-2');
    expect(auth.session()).toEqual(refreshed);
  });

  it('signOut calls the API and clears local session even when the API errors', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/sign-in`).flush(authBody);

    auth.signOut().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/sign-out`);
    expect(req.request.body).toEqual({ revokeAllSessions: false });
    req.flush(null, { status: 500, statusText: 'Server Error' });

    expect(auth.accessTokenReadonly()).toBeNull();
    expect(auth.session()).toBeNull();
  });
});
