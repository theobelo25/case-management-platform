import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../api/api-base-url.token';
import { Observable } from 'rxjs';

export interface SignInRequestDto {
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

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  signIn(body: SignInRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/sign-in`, body);
  }
}
