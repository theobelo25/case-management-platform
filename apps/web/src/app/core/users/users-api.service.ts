import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../api/api-base-url.token';
import { Observable } from 'rxjs';

export interface UserSearchHitDto {
  userId: string;
  fullName: string;
  email: string;
}

export interface UserSearchPageDto {
  items: UserSearchHitDto[];
  nextCursor: string;
  limit: number;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  searchUsers(q: string, cursor?: string | null, limit = 20): Observable<UserSearchPageDto> {
    let params = new HttpParams().set('q', q).set('limit', String(limit));
    if (cursor != null && cursor !== '') {
      params = params.set('cursor', cursor);
    }
    return this.http.get<UserSearchPageDto>(`${this.baseUrl}/users/search`, { params });
  }
}
