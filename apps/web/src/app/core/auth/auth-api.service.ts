import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { Observable } from 'rxjs';

export interface SignInRequestDto {
  email: string;
  password: string;
}

export interface SignUpRequestDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthResponseDto {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  fullName: string;
}

export interface SignOutRequestDto {
  revokeAllSessions?: boolean;
}

/** Matches `GET /auth/me` (ASP.NET camelCase JSON). */
export interface MeResponseDto {
  userId: string;
  email: string;
  fullName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  signIn(body: SignInRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/sign-in`, body);
  }

  signUp(body: SignUpRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/sign-up`, body);
  }

  refresh(): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/refresh`, {});
  }

  signOut(body: SignOutRequestDto = {}): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/sign-out`, body);
  }

  getMe(): Observable<MeResponseDto> {
    return this.http.get<MeResponseDto>(`${this.baseUrl}/auth/me`);
  }
}
