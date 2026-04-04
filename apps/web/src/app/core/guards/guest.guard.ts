import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
import { isInternalAppPath } from '@app/core/navigation/is-internal-app-path';
import { map } from 'rxjs';

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
    map(() => (auth.session() !== null ? redirectWhenSignedIn() : true)),
  );
};
