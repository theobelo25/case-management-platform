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
  confirmPassword: string;
}

export interface AccessTokenResponseDto {
  accessToken: string;
}

export interface AuthResponseDto extends AccessTokenResponseDto {
  expiresAtUtc: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface SignOutRequestDto {
  revokeAllSessions?: boolean;
}

export interface UpdateProfileRequestDto {
  firstName: string;
  lastName: string;
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

/** Matches `GET /auth/me` (ASP.NET camelCase JSON). */
export interface MeResponseDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  signIn(body: SignInRequestDto): Observable<AccessTokenResponseDto> {
    return this.http.post<AccessTokenResponseDto>(`${this.baseUrl}/auth/login`, body);
  }

  signUp(body: SignUpRequestDto): Observable<AccessTokenResponseDto> {
    return this.http.post<AccessTokenResponseDto>(`${this.baseUrl}/auth/register`, body);
  }

  refresh(): Observable<AccessTokenResponseDto> {
    return this.http.post<AccessTokenResponseDto>(`${this.baseUrl}/auth/refresh`, {});
  }

  signOut(body: SignOutRequestDto = {}): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/logout`, body);
  }

  getMe(): Observable<MeResponseDto> {
    return this.http.get<MeResponseDto>(`${this.baseUrl}/auth/me`);
  }

  updateProfile(body: UpdateProfileRequestDto): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/auth/me`, body);
  }
}
