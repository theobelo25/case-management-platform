import { HttpInterceptorFn } from '@angular/common/http';

/** Required so HttpOnly refresh cookie is stored/sent on cross-origin API (with correct CORS). */
export const withCredentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));
