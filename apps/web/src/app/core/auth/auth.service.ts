import { inject, Injectable, signal } from '@angular/core';
import {
  AuthApiService,
  AuthResponseDto,
  MeResponseDto,
  SignUpRequestDto,
  UpdateProfileRequestDto,
} from './auth-api.service';
import { parseSessionFromAccessToken } from './parse-access-token-session';
import {
  catchError,
  EMPTY,
  finalize,
  map,
  Observable,
  of,
  ReplaySubject,
  switchMap,
  take,
  tap,
} from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);

  private readonly accessToken = signal<string | null>(null);
  readonly accessTokenReadonly = this.accessToken.asReadonly();

  readonly session = signal<AuthResponseDto | null>(null);

  /** Populated from `GET /auth/me` after a valid access token is applied. */
  readonly userProfile = signal<MeResponseDto | null>(null);

  private readonly sessionRestorePhase = signal<'idle' | 'pending' | 'done'>('idle');
  private readonly sessionRestoreFinished$ = new ReplaySubject<void>(1);

  private applyAccessToken(accessToken: string): void {
    const parsed = parseSessionFromAccessToken(accessToken);
    this.accessToken.set(accessToken);
    if (!parsed) {
      this.session.set(null);
      this.userProfile.set(null);
      return;
    }
    this.session.set({ accessToken, ...parsed });
    this.loadUserProfile();
  }

  private loadUserProfile(): void {
    const expectedUserId = this.session()?.userId;
    if (!expectedUserId) {
      return;
    }

    this.api
      .getMe()
      .pipe(take(1), catchError(() => of(null)))
      .subscribe((profile) => {
        if (!profile) {
          return;
        }
        const currentUserId = this.session()?.userId;
        if (currentUserId && currentUserId === profile.id) {
          this.userProfile.set(profile);
        }
      });
  }

  signIn(payload: { email: string; password: string }): Observable<AuthResponseDto> {
    return this.api.signIn(payload).pipe(
      tap((res) => this.applyAccessToken(res.accessToken)),
      map(() => {
        const s = this.session();
        if (!s) {
          throw new Error('Invalid access token.');
        }
        return s;
      }),
    );
  }

  signUp(payload: SignUpRequestDto): Observable<AuthResponseDto> {
    return this.api.signUp(payload).pipe(
      tap((res) => this.applyAccessToken(res.accessToken)),
      map(() => {
        const s = this.session();
        if (!s) {
          throw new Error('Invalid access token.');
        }
        return s;
      }),
    );
  }

  updateProfile(payload: UpdateProfileRequestDto): Observable<AuthResponseDto> {
    return this.api.updateProfile(payload).pipe(switchMap(() => this.refreshSession()));
  }

  /** Updates active organization via `PATCH /auth/me` and reloads `userProfile` from `GET /auth/me`. */
  switchActiveOrganization(organizationId: string): Observable<void> {
    return this.api.patchProfile({ activeOrganizationId: organizationId }).pipe(
      switchMap(() => this.api.getMe().pipe(take(1))),
      tap((profile) => {
        if (!profile) {
          return;
        }
        const currentUserId = this.session()?.userId;
        if (currentUserId && currentUserId === profile.id) {
          this.userProfile.set(profile);
        }
      }),
      map(() => void 0),
    );
  }

  refreshSession(): Observable<AuthResponseDto> {
    return this.api.refresh().pipe(
      tap((res) => this.applyAccessToken(res.accessToken)),
      map(() => {
        const s = this.session();
        if (!s) {
          throw new Error('Invalid access token.');
        }
        return s;
      }),
    );
  }

  startSessionRestore(): void {
    if (this.sessionRestorePhase() !== 'idle') {
      return;
    }

    this.sessionRestorePhase.set('pending');

    this.refreshSession()
      .pipe(
        catchError(() => of(void 0)),
        finalize(() => {
          this.sessionRestorePhase.set('done');
          this.sessionRestoreFinished$.next();
        }),
      )
      .subscribe();
  }

  whenSessionRestored(): Observable<void> {
    this.startSessionRestore();
    if (this.sessionRestorePhase() === 'done') {
      return of(void 0);
    }
    return this.sessionRestoreFinished$.pipe(take(1));
  }

  clearLocalSession(): void {
    this.accessToken.set(null);
    this.session.set(null);
    this.userProfile.set(null);
  }

  signOut(options?: { revokeAllSessions?: boolean }): Observable<void> {
    return this.api.signOut({ revokeAllSessions: options?.revokeAllSessions ?? false }).pipe(
      catchError(() => EMPTY),
      finalize(() => {
        this.clearLocalSession();
      }),
    );
  }
}
