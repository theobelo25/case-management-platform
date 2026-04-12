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
        id: '11111111-1111-1111-1111-111111111111',
        email: 'a@b.c',
        firstName: 'Test',
        lastName: 'User',
        activeOrganizationId: '22222222-2222-2222-2222-222222222222',
        organizations: [
          { id: '22222222-2222-2222-2222-222222222222', name: 'Org', role: 'Member', isArchived: false },
        ],
      };

      service.getMe().subscribe((res) => {
        expect(res).toEqual(body);
      });

      const req = httpMock.expectOne(`${baseUrl}/auth/me`);
      expect(req.request.method).toBe('GET');
      expect(req.request.headers.get('Cache-Control')).toBe('no-cache');
      expect(req.request.headers.get('Pragma')).toBe('no-cache');
      req.flush(body);
    });
  });

  describe('signIn', () => {
    it('POSTs credentials to /auth/login', () => {
      const response = { accessToken: 't' };

      service.signIn({ email: 'a@b.c', password: 'secret' }).subscribe((res) => {
        expect(res).toEqual(response);
      });

      const req = httpMock.expectOne(`${baseUrl}/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'a@b.c', password: 'secret' });
      req.flush(response);
    });
  });
});
