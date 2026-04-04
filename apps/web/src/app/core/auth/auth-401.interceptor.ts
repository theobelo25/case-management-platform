import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { requestMatchesApiBaseUrl } from '@app/core/api/request-matches-api-base-url';
import { AuthService } from './auth.service';
import { AuthRefreshCoordinator } from './auth-refresh-coordinator.service';
import { catchError, noop, switchMap, throwError } from 'rxjs';

const RETRY_HEADER = 'X-Auth-Retry';
const SKIP_401_REFRESH = ['/auth/sign-in', '/auth/sign-up', '/auth/refresh', '/auth/sign-out'];

export const auth401Interceptor: HttpInterceptorFn = (req, next) => {
  const baseUrl = inject(API_BASE_URL);
  const auth = inject(AuthService);
  const refreshCoordinator = inject(AuthRefreshCoordinator);
  const http = inject(HttpClient);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401) {
        return throwError(() => err);
      }

      const url = req.url;
      if (!requestMatchesApiBaseUrl(url, baseUrl)) {
        return throwError(() => err);
      }
      if (SKIP_401_REFRESH.some((f) => url.includes(f))) {
        return throwError(() => err);
      }
      if (req.headers.has(RETRY_HEADER)) {
        return throwError(() => err);
      }

      return refreshCoordinator.refreshOnceShared().pipe(
        catchError((refreshErr) => {
          auth.signOut().subscribe({ complete: () => noop });
          return throwError(() => refreshErr);
        }),
        switchMap(() =>
          http.request(
            req.clone({
              setHeaders: { [RETRY_HEADER]: '1' },
            }),
          ),
        ),
      );
    }),
  );
};
