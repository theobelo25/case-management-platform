import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthApiService, MeResponseDto } from './auth-api.service';

describe('AuthApiService', () => {
  const baseUrl = 'http://api.test';
  let service: AuthApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    service = TestBed.inject(AuthApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getMe', () => {
    it('GETs /auth/me against the configured API base URL', () => {
      const body: MeResponseDto = {
        userId: '11111111-1111-1111-1111-111111111111',
        email: 'a@b.c',
        fullName: 'Test User',
      };

      service.getMe().subscribe((res) => {
        expect(res).toEqual(body);
      });

      const req = httpMock.expectOne(`${baseUrl}/auth/me`);
      expect(req.request.method).toBe('GET');
      req.flush(body);
    });
  });

  describe('signIn', () => {
    it('POSTs credentials to /auth/sign-in', () => {
      const response = {
        accessToken: 't',
        expiresAtUtc: '2026-01-01T00:00:00Z',
        userId: 'u',
        email: 'a@b.c',
        fullName: 'U',
      };

      service.signIn({ email: 'a@b.c', password: 'secret' }).subscribe((res) => {
        expect(res).toEqual(response);
      });

      const req = httpMock.expectOne(`${baseUrl}/auth/sign-in`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'a@b.c', password: 'secret' });
      req.flush(response);
    });
  });
});
