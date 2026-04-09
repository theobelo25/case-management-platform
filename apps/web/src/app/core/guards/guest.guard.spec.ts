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
import { guestGuard } from './guest.guard';
import { firstValueFrom, isObservable, of } from 'rxjs';

describe('guestGuard', () => {
  const sessionSignal = signal<AuthResponseDto | null>(null);

  const sessionDto: AuthResponseDto = {
    accessToken: 't',
    expiresAtUtc: '2026-01-01T00:00:00Z',
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    firstName: 'Test',
    lastName: 'User',
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

  async function runGuard(queryParams: Record<string, string>) {
    const route = {
      queryParamMap: {
        get: (k: string) => queryParams[k] ?? null,
      },
    } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() =>
      guestGuard(route, {} as RouterStateSnapshot),
    );
    if (isObservable(result)) {
      return firstValueFrom(result);
    }
    return result;
  }

  it('allows activation when there is no session', async () => {
    expect(await runGuard({})).toBe(true);
  });

  it('redirects to /app when signed in and no returnUrl', async () => {
    sessionSignal.set(sessionDto);
    const router = TestBed.inject(Router);
    const result = await runGuard({});

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(
      router.serializeUrl(router.parseUrl('/app')),
    );
  });

  it('redirects to returnUrl when signed in and returnUrl is internal', async () => {
    sessionSignal.set(sessionDto);
    const router = TestBed.inject(Router);
    const result = await runGuard({ returnUrl: '/app/settings' });

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(
      router.serializeUrl(router.parseUrl('/app/settings')),
    );
  });

  it('ignores unsafe returnUrl when signed in', async () => {
    sessionSignal.set(sessionDto);
    const router = TestBed.inject(Router);
    const result = await runGuard({ returnUrl: '//evil.example/phish' });

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(
      router.serializeUrl(router.parseUrl('/app')),
    );
  });
});
