import { inject, Injectable, signal } from '@angular/core';
import { AuthApiService, AuthResponseDto, SignUpRequestDto } from './auth-api.service';
import { catchError, EMPTY, finalize, Observable, of, ReplaySubject, take, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);

  private readonly accessToken = signal<string | null>(null);
  readonly accessTokenReadonly = this.accessToken.asReadonly();

  readonly session = signal<AuthResponseDto | null>(null);

  private readonly sessionRestorePhase = signal<'idle' | 'pending' | 'done'>('idle');
  private readonly sessionRestoreFinished$ = new ReplaySubject<void>(1);

  private applyAuthResponse(res: AuthResponseDto): void {
    this.accessToken.set(res.accessToken);
    this.session.set(res);
  }

  signIn(payload: { email: string; password: string }): Observable<AuthResponseDto> {
    return this.api.signIn(payload).pipe(tap((res) => this.applyAuthResponse(res)));
  }

  signUp(payload: SignUpRequestDto): Observable<AuthResponseDto> {
    return this.api.signUp(payload).pipe(tap((res) => this.applyAuthResponse(res)));
  }

  refreshSession(): Observable<AuthResponseDto> {
    return this.api.refresh().pipe(tap((res) => this.applyAuthResponse(res)));
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
