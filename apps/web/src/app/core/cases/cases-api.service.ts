import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../api/api-base-url.token';
import { CasePriority, CaseStatus } from '@app/features/cases/models/cases.types';
import { priorityToApiCode, statusToApiCode } from '@app/features/cases/models/case-code-maps';
import { Observable } from 'rxjs';

export interface CaseCreatorResponseDto {
  id: string;
  email: string;
  displayName: string;
}

export interface CaseResponseDto {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdByUserId: string;
  createdBy: CaseCreatorResponseDto;
}

/** Response shape for GET /cases (cursor pagination). */
export interface CaseListResponseDto {
  items: CaseResponseDto[];
  nextCursor: string | null;
  previousCursor: string | null;
}

export interface CaseListQueryParams {
  limit?: number;
  cursor?: string;
  search?: string;
  priority?: string;
  status?: string;
  sort?: string;
  sortDescending?: boolean;
}

export interface addCaseRequestDto {
  title: string;
  description: string;
  priority: CasePriority;
  status: CaseStatus;
}

@Injectable({ providedIn: 'root' })
export class CasesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  addCase(body: addCaseRequestDto): Observable<CaseResponseDto> {
    return this.http.post<CaseResponseDto>(`${this.baseUrl}/cases`, {
      title: body.title,
      description: body.description,
      priority: priorityToApiCode(body.priority),
    });
  }

  updateCase(body: addCaseRequestDto): Observable<CaseResponseDto> {
    return this.http.put<CaseResponseDto>(`${this.baseUrl}/cases`, {
      title: body.title,
      description: body.description,
      priority: priorityToApiCode(body.priority),
      status: statusToApiCode(body.status),
    });
  }

  getCases(params?: CaseListQueryParams): Observable<CaseListResponseDto> {
    return this.http.get<CaseListResponseDto>(`${this.baseUrl}/cases`, {
      params: this.buildCaseListQueryParams(params),
    });
  }

  private buildCaseListQueryParams(params?: CaseListQueryParams): HttpParams {
    if (!params) {
      return new HttpParams();
    }
    let httpParams = new HttpParams();
    const append = (key: string, value: string | number | boolean): void => {
      httpParams = httpParams.set(key, String(value));
    };
    if (params.limit != null) {
      append('limit', params.limit);
    }
    if (params.cursor != null && params.cursor !== '') {
      append('cursor', params.cursor);
    }
    if (params.search != null && params.search !== '') {
      append('search', params.search);
    }
    if (params.priority != null && params.priority !== '') {
      append('priority', params.priority);
    }
    if (params.status != null && params.status !== '') {
      append('status', params.status);
    }
    if (params.sort != null && params.sort !== '') {
      append('sort', params.sort);
    }
    if (params.sortDescending != null) {
      append('sortDescending', params.sortDescending);
    }
    return httpParams;
  }

  getCase(id: string): Observable<CaseResponseDto> {
    return this.http.get<CaseResponseDto>(`${this.baseUrl}/cases/${id}`);
  }
}
