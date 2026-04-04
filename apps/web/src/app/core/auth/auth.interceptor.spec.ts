import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  const baseUrl = 'http://api.test';
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
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

  it('adds Authorization when a token exists and the URL is under the API base', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/sign-in`).flush({
      accessToken: 'abc',
      expiresAtUtc: '2026-01-01T00:00:00Z',
      userId: 'u',
      email: 'u@test.com',
      fullName: 'U',
    });

    http.get(`${baseUrl}/auth/me`).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/me`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc');
    req.flush({ userId: 'u', email: 'u@test.com', fullName: 'U' });
  });

  it('does not attach Bearer to sign-in, sign-up, or refresh', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/sign-in`).flush({
      accessToken: 'abc',
      expiresAtUtc: '2026-01-01T00:00:00Z',
      userId: 'u',
      email: 'u@test.com',
      fullName: 'U',
    });

    http.post(`${baseUrl}/auth/refresh`, {}).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({
      accessToken: 'new',
      expiresAtUtc: '2026-01-02T00:00:00Z',
      userId: 'u',
      email: 'u@test.com',
      fullName: 'U',
    });
  });

  it('does not attach Bearer to requests outside the API base URL', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/sign-in`).flush({
      accessToken: 'abc',
      expiresAtUtc: '2026-01-01T00:00:00Z',
      userId: 'u',
      email: 'u@test.com',
      fullName: 'U',
    });

    http.get('https://other.example/data').subscribe();

    const req = httpMock.expectOne('https://other.example/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not attach Bearer when there is no session token', () => {
    http.get(`${baseUrl}/auth/me`).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/me`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({ userId: 'u', email: 'u@test.com', fullName: 'U' });
  });
});
