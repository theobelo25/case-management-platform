import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.session() !== null) {
    return true;
  }

  return auth.whenSessionRestored().pipe(
    map(() => {
      if (auth.session() !== null) {
        return true;
      }
      return router.createUrlTree(['/auth', 'sign-in'], {
        queryParams: { returnUrl: state.url },
      });
    }),
  );
};
