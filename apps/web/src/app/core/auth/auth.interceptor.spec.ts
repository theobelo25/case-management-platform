import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';
import { encodeTestAccessToken } from './test-access-token';

describe('authInterceptor', () => {
  const baseUrl = 'http://api.test';
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: AuthService;

  const meBody = {
    id: 'u',
    email: 'u@test.com',
    firstName: 'U',
    lastName: '',
    activeOrganizationId: '22222222-2222-2222-2222-222222222222',
    organizations: [
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Test Org',
        role: 'Administrator',
        isArchived: false,
      },
    ],
  };

  const testToken = encodeTestAccessToken({
    sub: 'u',
    email: 'u@test.com',
    given_name: 'U',
    family_name: '',
    exp: 4102444800,
  });

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
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: testToken });

    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meBody);

    http.get(`${baseUrl}/auth/me`).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/me`);
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${testToken}`);
    req.flush(meBody);
  });

  it('does not attach Bearer to login, register, or refresh', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: testToken });
    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meBody);

    http.post(`${baseUrl}/auth/refresh`, {}).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/refresh`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({ accessToken: testToken });
  });

  it('does not attach Bearer to requests outside the API base URL', () => {
    auth.signIn({ email: 'u@test.com', password: 'p' }).subscribe();
    httpMock.expectOne(`${baseUrl}/auth/login`).flush({ accessToken: testToken });
    httpMock.expectOne(`${baseUrl}/auth/me`).flush(meBody);

    http.get('https://other.example/data').subscribe();

    const req = httpMock.expectOne('https://other.example/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not attach Bearer when there is no session token', () => {
    http.get(`${baseUrl}/auth/me`).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/auth/me`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush(meBody);
  });
});
