import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import type { AuthResponseDto } from '@app/core/auth/auth-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { authGuard } from './auth.guard';
import { firstValueFrom, isObservable, of } from 'rxjs';

describe('authGuard', () => {
  const sessionSignal = signal<AuthResponseDto | null>(null);

  const sessionDto: AuthResponseDto = {
    accessToken: 't',
    expiresAtUtc: '2026-01-01T00:00:00Z',
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    fullName: 'Test User',
  };

  beforeEach(() => {
    sessionSignal.set(null);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            session: sessionSignal.asReadonly(),
            whenSessionRestored: () => of(void 0),
          },
        },
      ],
    });
  });

  async function runGuard(url: string) {
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
    if (isObservable(result)) {
      return firstValueFrom(result);
    }
    return result;
  }

  it('allows activation when a session exists', async () => {
    sessionSignal.set(sessionDto);

    expect(await runGuard('/dashboard')).toBe(true);
  });

  it('redirects to sign-in with returnUrl when there is no session', async () => {
    const router = TestBed.inject(Router);
    const result = await runGuard('/dashboard');

    expect(result).toBeInstanceOf(UrlTree);
    const expected = router.createUrlTree(['/auth', 'sign-in'], {
      queryParams: { returnUrl: '/dashboard' },
    });
    expect(router.serializeUrl(result as UrlTree)).toBe(router.serializeUrl(expected));
  });
});
