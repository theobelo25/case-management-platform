import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
import { isInternalAppPath } from '@app/core/navigation/is-internal-app-path';
import { catchError, map, of, timeout } from 'rxjs';

/** Session restore uses HttpClient (no default timeout); cap wait so auth routes still load offline. */
const SESSION_RESTORE_WAIT_MS = 12_000;

export const guestGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const redirectWhenSignedIn = () => {
    const raw = route.queryParamMap.get('returnUrl');
    const path = raw && isInternalAppPath(raw) ? raw : '/app';
    return router.parseUrl(path);
  };

  if (auth.session() !== null) {
    return redirectWhenSignedIn();
  }

  return auth.whenSessionRestored().pipe(
    timeout(SESSION_RESTORE_WAIT_MS),
    catchError(() => of(void 0)),
    map(() => (auth.session() !== null ? redirectWhenSignedIn() : true)),
  );
};
