import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../api/api-base-url.token';
import { map, Observable } from 'rxjs';

export interface CreateOrganizationRequestDto {
  name: string;
}

export interface OrganizationResponseDto {
  id: string;
  name: string;
  createdAtUtc: string;
  isArchived: boolean;
}

export interface UserMembershipResponseDto {
  id: string;
  name: string;
  role: string;
  isArchived: boolean;
  email: string;
  joinedAtUtc: string;
}

export interface PagedUserMembershipsResponseDto {
  items: UserMembershipResponseDto[];
  totalCount: number;
  skip: number;
  limit: number;
  hasMore: boolean;
}

export interface OrganizationSlaPolicyDto {
  lowHours: number;
  mediumHours: number;
  highHours: number;
}

export interface OrganizationDetailsResponseDto {
  organization: OrganizationResponseDto;
  slaPolicy: OrganizationSlaPolicyDto;
  members: UserMembershipResponseDto[];
}

export interface TransferOwnershipRequestDto {
  newOwnerUserId: string;
}

@Injectable({ providedIn: 'root' })
export class OrganizationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listUserOrganizations(skip = 0, limit = 10): Observable<PagedUserMembershipsResponseDto> {
    const params = new HttpParams().set('skip', String(skip)).set('limit', String(limit));

    return this.http.get<PagedUserMembershipsResponseDto>(`${this.baseUrl}/organizations`, {
      params,
    });
  }

  createOrganization(body: CreateOrganizationRequestDto): Observable<OrganizationResponseDto> {
    return this.http.post<OrganizationResponseDto>(`${this.baseUrl}/organizations`, body);
  }

  getOrganizationDetails(id: string): Observable<OrganizationDetailsResponseDto> {
    return this.http.get<unknown>(`${this.baseUrl}/organizations/${id}`).pipe(
      map((raw) => normalizeOrganizationDetailsResponse(raw)),
    );
  }

  archiveOrganization(id: string): Observable<OrganizationResponseDto> {
    return this.http.patch<OrganizationResponseDto>(
      `${this.baseUrl}/organizations/${id}/archive`,
      {},
    );
  }

  unarchiveOrganization(id: string): Observable<OrganizationResponseDto> {
    return this.http.patch<OrganizationResponseDto>(
      `${this.baseUrl}/organizations/${id}/unarchive`,
      {},
    );
  }

  deleteOrganization(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/organizations/${id}`);
  }

  transferOrganizationOwnership(
    organizationId: string,
    body: TransferOwnershipRequestDto,
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/organizations/${organizationId}/transfer-ownership`,
      body,
    );
  }

  addOrganizationMember(organizationId: string, memberUserId: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/organizations/${organizationId}/members/${memberUserId}`,
      {},
    );
  }

  removeOrganizationMember(organizationId: string, memberUserId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/organizations/${organizationId}/members/${memberUserId}`,
    );
  }

  updateOrganizationSlaPolicy(
    organizationId: string,
    body: OrganizationSlaPolicyDto,
  ): Observable<OrganizationSlaPolicyDto> {
    return this.http
      .put<unknown>(`${this.baseUrl}/organizations/${organizationId}/sla-policy`, {
        lowHours: body.lowHours,
        mediumHours: body.mediumHours,
        highHours: body.highHours,
      })
      .pipe(map((raw) => normalizeSlaPolicy(raw)));
  }
}

/** Supports camelCase / PascalCase and `id` vs `userId` from the API. */
function normalizeOrganizationDetailsResponse(raw: unknown): OrganizationDetailsResponseDto {
  if (raw == null || typeof raw !== 'object') {
    return emptyOrgDetail();
  }

  const r = raw as Record<string, unknown>;
  const orgRaw = r['organization'] ?? r['Organization'];
  const membersRaw = r['members'] ?? r['Members'];
  const slaRaw = r['slaPolicy'] ?? r['SlaPolicy'];

  const organization =
    orgRaw != null && typeof orgRaw === 'object'
      ? normalizeOrganization(orgRaw as Record<string, unknown>)
      : { id: '', name: '', createdAtUtc: '', isArchived: false };

  let members: UserMembershipResponseDto[] = [];
  if (Array.isArray(membersRaw)) {
    members = membersRaw.map((row) => normalizeMembershipRow(row));
  }

  const slaPolicy = normalizeSlaPolicy(slaRaw);

  return { organization, slaPolicy, members };
}

function normalizeSlaPolicy(raw: unknown): OrganizationSlaPolicyDto {
  const defaults: OrganizationSlaPolicyDto = { lowHours: 24, mediumHours: 8, highHours: 4 };
  if (raw == null || typeof raw !== 'object') return defaults;
  const s = raw as Record<string, unknown>;
  return {
    lowHours: numOr(s['lowHours'] ?? s['LowHours'], defaults.lowHours),
    mediumHours: numOr(s['mediumHours'] ?? s['MediumHours'], defaults.mediumHours),
    highHours: numOr(s['highHours'] ?? s['HighHours'], defaults.highHours),
  };
}

function numOr(v: unknown, fallback: number): number {
  if (typeof v === 'number' && Number.isFinite(v)) return v;
  if (typeof v === 'string' && v.trim() !== '') {
    const n = Number(v);
    if (Number.isFinite(n)) return n;
  }
  return fallback;
}

function emptyOrgDetail(): OrganizationDetailsResponseDto {
  return {
    organization: { id: '', name: '', createdAtUtc: '', isArchived: false },
    slaPolicy: { lowHours: 24, mediumHours: 8, highHours: 4 },
    members: [],
  };
}

function normalizeOrganization(o: Record<string, unknown>): OrganizationResponseDto {
  return {
    id: String(o['id'] ?? o['Id'] ?? ''),
    name: String(o['name'] ?? o['Name'] ?? ''),
    createdAtUtc: String(o['createdAtUtc'] ?? o['CreatedAtUtc'] ?? ''),
    isArchived: Boolean(o['isArchived'] ?? o['IsArchived'] ?? false),
  };
}

function normalizeMembershipRow(row: unknown): UserMembershipResponseDto {
  if (row == null || typeof row !== 'object') {
    return { id: '', name: '', role: '', isArchived: false, email: '', joinedAtUtc: '' };
  }
  const m = row as Record<string, unknown>;
  return {
    id: String(m['id'] ?? m['userId'] ?? m['Id'] ?? m['UserId'] ?? ''),
    name: String(m['name'] ?? m['Name'] ?? ''),
    role: String(m['role'] ?? m['Role'] ?? ''),
    isArchived: Boolean(m['isArchived'] ?? m['IsArchived'] ?? false),
    email: String(m['email'] ?? m['Email'] ?? ''),
    joinedAtUtc: String(m['joinedAtUtc'] ?? m['JoinedAtUtc'] ?? ''),
  };
}
