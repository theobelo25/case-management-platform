import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError, finalize, map, Observable, shareReplay, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthRefreshCoordinator {
  private readonly auth = inject(AuthService);
  private inFlight: Observable<void> | null = null;

  refreshOnceShared(): Observable<void> {
    if (!this.inFlight) {
      this.inFlight = this.auth.refreshSession().pipe(
        map(() => void 0),
        catchError((err) => {
          this.inFlight = null;
          return throwError(() => err);
        }),
        finalize(() => {
          this.inFlight = null;
        }),
        shareReplay({ bufferSize: 1, refCount: true }),
      );
    }
    return this.inFlight;
  }
}
