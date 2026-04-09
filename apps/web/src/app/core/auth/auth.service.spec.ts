import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';
import { encodeTestAccessToken } from './test-access-token';

describe('AuthService', () => {
  const baseUrl = 'http://api.test';
  let auth: AuthService;
  let httpMock: HttpTestingController;

  const token1 = encodeTestAccessToken({
    sub: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    given_name: 'Test',
    family_name: 'User',
    exp: 4102444800,
  });

  const token2 = encodeTestAccessToken({
    sub: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    given_name: 'Test',
    family_name: 'User',
    exp: 4102444900,
  });

  const sessionFromToken1 = {
    accessToken: token1,
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    firstName: 'Test',
    lastName: 'User',
    expiresAtUtc: new Date(4102444800 * 1000).toISOString(),
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

  it('signIn stores access token and session derived from the JWT', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/login`);
    req.flush({ accessToken: token1 });

    expect(auth.accessTokenReadonly()).toBe(token1);
    expect(auth.session()).toEqual(sessionFromToken1);
  });

  it('refreshSession replaces token and session', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: token1 });

    auth.refreshSession().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    req.flush({ accessToken: token2 });

    expect(auth.accessTokenReadonly()).toBe(token2);
    expect(auth.session()?.accessToken).toBe(token2);
    expect(auth.session()?.expiresAtUtc).toBe(new Date(4102444900 * 1000).toISOString());
  });

  it('signOut calls the API and clears local session even when the API errors', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: token1 });

    auth.signOut().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/logout`);
    expect(req.request.body).toEqual({ revokeAllSessions: false });
    req.flush(null, { status: 500, statusText: 'Server Error' });

    expect(auth.accessTokenReadonly()).toBeNull();
    expect(auth.session()).toBeNull();
  });
});
