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
  /** Present on case detail; supports camelCase / PascalCase from the API. */
  organizationId?: string;
  title: string;
  description?: string;
  status: string;
  priority: string;
  /**
   * SLA lifecycle from the API (`CaseListItemResponse` / `CaseDetailResponse`).
   * Codes: `NONE`, `ACTIVE`, `OVERDUE`, `PAUSED`, `BREACHED`.
   */
  slaState?: string;
  slaDueAtUtc?: string | null;
  slaBreachedAtUtc?: string | null;
  slaPausedAtUtc?: string | null;
  /** Remaining SLA budget in seconds when the clock is running. */
  slaRemainingSeconds?: number | null;
  isArchived?: boolean;
  requesterUserId?: string | null;
  requesterName?: string | null;
  assigneeUserId?: string | null;
  assigneeName?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdByUserId: string;
  /** Display name from API (User.FullName); prefer over raw id. */
  createdByName?: string | null;
  createdBy?: CaseCreatorResponseDto;
  timeline?: CaseTimelineItemResponseDto[];
}

export interface CaseTimelineItemResponseDto {
  type: string;
  id: string;
  createdAtUtc: string;
  authorUserId: string | null;
  authorDisplayName?: string | null;
  body: string | null;
  isInternal: boolean | null;
  isInitial: boolean | null;
  eventType: string | null;
  metadata: string | null;
}

/** Response shape for GET /cases (cursor pagination). */
export interface CaseListResponseDto {
  items: CaseResponseDto[];
  nextCursor: string | null;
  previousCursor: string | null;
}

/** GET /cases/volume-over-time — daily counts in the active organization (UTC dates). */
export interface CaseVolumeDayPointDto {
  date: string;
  casesCreated: number;
  casesResolved: number;
  casesReopened: number;
}

export interface CaseVolumeOverTimeDto {
  series: CaseVolumeDayPointDto[];
}

/** GET /cases/first-response-time-over-time — daily average first response (UTC). */
export interface FirstResponseTimeDayPointDto {
  date: string;
  averageFirstResponseMinutes: number | null;
  casesWithFirstResponse: number;
}

export interface FirstResponseTimeOverTimeDto {
  series: FirstResponseTimeDayPointDto[];
}

/** GET /cases/count-by-status — non-archived cases in the active organization. */
export interface CaseStatusCountsDto {
  newCount: number;
  openCount: number;
  pendingCount: number;
  resolvedCount: number;
  closedCount: number;
}

export interface CaseListQueryParams {
  limit?: number;
  cursor?: string;
  search?: string;
  priority?: string;
  status?: string;
  sort?: string;
  sortDescending?: boolean;
  /** When true, only cases assigned to the current user (active organization). */
  assignedToMe?: boolean;
  /** When true, only cases past SLA due (non-terminal status). */
  overdueOnly?: boolean;
  /** When true, only cases with a persisted SLA breach. */
  breachedOnly?: boolean;
  /** When true, only cases with no assignee. */
  unassignedOnly?: boolean;
  /** SLA due within the next N hours (1–720); combine with `assignedToMe` for “my due soon”. */
  dueSoonWithinHours?: number;
}

export type BulkCaseActionApi = 'ASSIGN' | 'SET_PRIORITY' | 'SET_STATUS' | 'BUMP_PRIORITY';

export interface BulkCasesRequestDto {
  caseIds: string[];
  action: BulkCaseActionApi;
  assigneeUserId?: string | null;
  priority?: string;
  status?: string;
}

export interface BulkCasesResultDto {
  updatedCount: number;
}

export interface addCaseRequestDto {
  title: string;
  description: string;
  priority: CasePriority;
  status: CaseStatus;
  /** When set, links the case to this org member as requester (create only). */
  requesterUserId?: string | null;
}

export interface updateCaseMetadataRequestDto {
  caseId: string;
  status: CaseStatus;
  priority: CasePriority;
}

export interface addCaseCommentRequestDto {
  caseId: string;
  body: string;
  isInternal: boolean;
}

export interface assignCaseRequestDto {
  caseId: string;
  assigneeUserId: string | null;
}

@Injectable({ providedIn: 'root' })
export class CasesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  addCase(body: addCaseRequestDto): Observable<CaseResponseDto> {
    const payload: {
      title: string;
      initialMessage: string;
      priority: ReturnType<typeof priorityToApiCode>;
      requesterUserId?: string;
    } = {
      title: body.title,
      initialMessage: body.description,
      priority: priorityToApiCode(body.priority),
    };
    const rid = body.requesterUserId?.trim();
    if (rid) {
      payload.requesterUserId = rid;
    }
    return this.http.post<CaseResponseDto>(`${this.baseUrl}/cases`, payload);
  }

  updateCase(body: addCaseRequestDto): Observable<CaseResponseDto> {
    return this.http.put<CaseResponseDto>(`${this.baseUrl}/cases`, {
      title: body.title,
      description: body.description,
      priority: priorityToApiCode(body.priority),
      status: statusToApiCode(body.status),
    });
  }

  updateCaseMetadata(body: updateCaseMetadataRequestDto): Observable<CaseResponseDto> {
    return this.http.patch<CaseResponseDto>(`${this.baseUrl}/cases/${body.caseId}`, {
      priority: priorityToApiCode(body.priority),
      status: statusToApiCode(body.status),
    });
  }

  addCaseComment(body: addCaseCommentRequestDto): Observable<CaseResponseDto> {
    return this.http.post<CaseResponseDto>(`${this.baseUrl}/cases/${body.caseId}/comments`, {
      body: body.body,
      isInternal: body.isInternal,
    });
  }

  assignCase(body: assignCaseRequestDto): Observable<CaseResponseDto> {
    return this.http.patch<CaseResponseDto>(`${this.baseUrl}/cases/${body.caseId}/assignee`, {
      assigneeUserId: body.assigneeUserId,
    });
  }

  getCases(params?: CaseListQueryParams): Observable<CaseListResponseDto> {
    return this.http.get<CaseListResponseDto>(`${this.baseUrl}/cases`, {
      params: this.buildCaseListQueryParams(params),
    });
  }

  /** `days`: 1–90, default 30. Scoped to JWT active organization. */
  getCaseVolumeOverTime(days = 30): Observable<CaseVolumeOverTimeDto> {
    const d = Math.min(90, Math.max(1, days));
    return this.http.get<CaseVolumeOverTimeDto>(`${this.baseUrl}/cases/volume-over-time`, {
      params: new HttpParams().set('days', String(d)),
    });
  }

  /** `days`: 1–90, default 30. Scoped to JWT active organization. */
  getFirstResponseTimeOverTime(days = 30): Observable<FirstResponseTimeOverTimeDto> {
    const d = Math.min(90, Math.max(1, days));
    return this.http.get<FirstResponseTimeOverTimeDto>(`${this.baseUrl}/cases/first-response-time-over-time`, {
      params: new HttpParams().set('days', String(d)),
    });
  }

  getCaseStatusCounts(): Observable<CaseStatusCountsDto> {
    return this.http.get<CaseStatusCountsDto>(`${this.baseUrl}/cases/count-by-status`);
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
    if (params.assignedToMe === true) {
      httpParams = httpParams.set('assignedToMe', 'true');
    }
    if (params.overdueOnly === true) {
      httpParams = httpParams.set('overdueOnly', 'true');
    }
    if (params.breachedOnly === true) {
      httpParams = httpParams.set('breachedOnly', 'true');
    }
    if (params.unassignedOnly === true) {
      httpParams = httpParams.set('unassignedOnly', 'true');
    }
    if (params.dueSoonWithinHours != null) {
      append('dueSoonWithinHours', params.dueSoonWithinHours);
    }
    return httpParams;
  }

  bulkCases(body: BulkCasesRequestDto): Observable<BulkCasesResultDto> {
    return this.http.post<BulkCasesResultDto>(`${this.baseUrl}/cases/bulk`, body);
  }

  getCase(id: string): Observable<CaseResponseDto> {
    return this.http.get<CaseResponseDto>(`${this.baseUrl}/cases/${id}`, {
      params: new HttpParams().set('_ts', Date.now()),
    });
  }

  deleteCase(caseId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/cases/${caseId}`);
  }

  archiveCase(caseId: string): Observable<CaseResponseDto> {
    return this.http.post<CaseResponseDto>(`${this.baseUrl}/cases/${caseId}/archive`, {});
  }

  unarchiveCase(caseId: string): Observable<CaseResponseDto> {
    return this.http.post<CaseResponseDto>(`${this.baseUrl}/cases/${caseId}/unarchive`, {});
  }
}
