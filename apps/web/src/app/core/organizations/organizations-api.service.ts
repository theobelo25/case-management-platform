import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../api/api-base-url.token';
import { Observable } from 'rxjs';

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

export interface OrganizationDetailsResponseDto {
  organization: OrganizationResponseDto;
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
    return this.http.get<OrganizationDetailsResponseDto>(`${this.baseUrl}/organizations/${id}`);
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
}
