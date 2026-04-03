import { inject, Injectable, signal } from '@angular/core';
import { AuthApiService, AuthResponseDto } from './auth-api.service';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);

  private readonly accessToken = signal<string | null>(null);
  readonly accessTokenReadonly = this.accessToken.asReadonly();

  readonly session = signal<AuthResponseDto | null>(null);

  signIn(payload: {
    email: string;
    password: string;
    rememberMe: boolean;
  }): Observable<AuthResponseDto> {
    const { email, password, rememberMe } = payload;
    console.log(rememberMe);
    return this.api.signIn({ email, password }).pipe(
      tap((res) => {
        this.accessToken.set(res.accessToken);
        this.session.set(res);
      }),
    );
  }

  signOut(): void {
    this.accessToken.set(null);
    this.session.set(null);
  }
}
