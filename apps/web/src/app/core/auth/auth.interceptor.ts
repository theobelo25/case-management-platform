import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { API_BASE_URL } from '../api/api-base-url.token';

const AUTH_SKIP_SUBSTRINGS = ['/auth/sign-in', '/auth/sign-up', '/auth/refresh'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const baseUrl = inject(API_BASE_URL);

  const token = auth.accessTokenReadonly();
  const url = req.url;

  const isOurApi = url.startsWith(baseUrl);
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
