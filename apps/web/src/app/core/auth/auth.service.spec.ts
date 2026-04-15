import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';
import type { MeResponseDto } from './auth-api.service';
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

  const meProfile: MeResponseDto = {
    id: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    firstName: 'Test',
    lastName: 'User',
    activeOrganizationId: '22222222-2222-2222-2222-222222222222',
    organizations: [
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Acme Legal',
        role: 'Administrator',
        isArchived: false,
      },
    ],
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

    const meReq = httpMock.expectOne(`${baseUrl}/auth/me`);
    meReq.flush(meProfile);

    expect(auth.accessTokenReadonly()).toBe(token1);
    expect(auth.session()).toEqual(sessionFromToken1);
    expect(auth.userProfile()).toEqual(meProfile);
  });

  it('refreshSession replaces token and session', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: token1 });
    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meProfile);

    auth.refreshSession().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    req.flush({ accessToken: token2 });

    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meProfile);

    expect(auth.accessTokenReadonly()).toBe(token2);
    expect(auth.session()?.accessToken).toBe(token2);
    expect(auth.session()?.expiresAtUtc).toBe(new Date(4102444900 * 1000).toISOString());
    expect(auth.userProfile()).toEqual(meProfile);
  });

  it('signOut calls the API and clears local session even when the API errors', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: token1 });
    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meProfile);

    auth.signOut().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/logout`);
    expect(req.request.body).toEqual({ revokeAllSessions: false });
    req.flush(null, { status: 500, statusText: 'Server Error' });

    expect(auth.accessTokenReadonly()).toBeNull();
    expect(auth.session()).toBeNull();
    expect(auth.userProfile()).toBeNull();
  });
});
