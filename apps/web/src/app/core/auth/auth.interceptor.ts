import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { requestMatchesApiBaseUrl } from '@app/core/api/request-matches-api-base-url';

const AUTH_SKIP_SUBSTRINGS = ['/auth/login', '/auth/register', '/auth/refresh'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const baseUrl = inject(API_BASE_URL);

  const token = auth.accessTokenReadonly();
  const url = req.url;

  const isOurApi = requestMatchesApiBaseUrl(url, baseUrl);
  const skipAuthHeader = AUTH_SKIP_SUBSTRINGS.some((fragment) => url.includes(fragment));

  if (!token || !isOurApi || skipAuthHeader) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    }),
  );
};
