import { inject, Injectable, signal } from '@angular/core';
import { AuthService } from '@app/core/auth/auth.service';
import {
  CreateOrganizationRequestDto,
  OrganizationDetailsResponseDto,
  OrganizationResponseDto,
  OrganizationSlaPolicyDto,
  OrganizationsApiService,
  UserMembershipResponseDto,
} from './organizations-api.service';
import { catchError, finalize, map, Observable, of, switchMap, tap, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class OrganizationsService {
  private readonly api = inject(OrganizationsApiService);
  private readonly auth = inject(AuthService);

  readonly organizations = signal<UserMembershipResponseDto[] | null>(null);
  readonly activeOrganization = signal<OrganizationResponseDto | null>(null);

  readonly createError = signal<string | null>(null);
  readonly loadError = signal<string | null>(null);

  readonly isCreating = signal(false);
  readonly isLoading = signal(false);

  readonly organizationDetail = signal<OrganizationDetailsResponseDto | null>(null);
  readonly detailLoadError = signal<string | null>(null);
  readonly isLoadingDetail = signal(false);

  readonly organizationListMeta = signal<{
    totalCount: number;
    skip: number;
    limit: number;
    hasMore: boolean;
  } | null>(null);

  listOrganizationsForUser(skip = 0, limit = 10): Observable<UserMembershipResponseDto[]> {
    return this.api.listUserOrganizations(skip, limit).pipe(
      tap((page) => {
        this.organizations.set(page.items);
        this.organizationListMeta.set({
          totalCount: page.totalCount,
          skip: page.skip,
          limit: page.limit,
          hasMore: page.hasMore,
        });
      }),
      map((page) => page.items),
      catchError((err: unknown) => {
        this.loadError.set(this.messageFromHttp(err));
        return throwError(() => err);
      }),
      finalize(() => this.isLoading.set(false)),
    );
  }

  clearCreateError(): void {
    this.createError.set(null);
  }

  clearLoadError(): void {
    this.loadError.set(null);
  }

  clearDetailLoadError(): void {
    this.detailLoadError.set(null);
  }

  createOrganization(body: CreateOrganizationRequestDto): Observable<OrganizationResponseDto> {
    this.clearCreateError();
    this.isCreating.set(true);
    return this.api.createOrganization(body).pipe(
      tap((organization) => this.activeOrganization.set(organization)),
      switchMap((organization) =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.clearLoadError();
            this.auth.refreshUserProfile();
          }),
          map(() => organization),
          catchError((err: unknown) => {
            this.loadError.set(this.messageFromHttp(err));
            return of(organization);
          }),
        ),
      ),
      catchError((err: unknown) => {
        this.createError.set(this.messageFromHttp(err));
        return throwError(() => err);
      }),
      finalize(() => this.isCreating.set(false)),
    );
  }

  getOrganizationDetails(id: string): Observable<OrganizationDetailsResponseDto> {
    this.clearDetailLoadError();
    this.isLoadingDetail.set(true);
    return this.api.getOrganizationDetails(id).pipe(
      tap((detail) => this.organizationDetail.set(detail)),
      catchError((err: unknown) => {
        this.detailLoadError.set(this.messageFromHttp(err));
        return throwError(() => err);
      }),
      finalize(() => this.isLoadingDetail.set(false)),
    );
  }

  getOrganizationDetailsReadonly(id: string): Observable<OrganizationDetailsResponseDto> {
    return this.api.getOrganizationDetails(id);
  }

  archiveOrganization(id: string): Observable<OrganizationResponseDto> {
    return this.api.archiveOrganization(id).pipe(
      switchMap((org) =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.auth.refreshUserProfile();
          }),
          map(() => org),
        ),
      ),
    );
  }

  unarchiveOrganization(id: string): Observable<OrganizationResponseDto> {
    return this.api.unarchiveOrganization(id).pipe(
      switchMap((org) =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.auth.refreshUserProfile();
          }),
          map(() => org),
        ),
      ),
    );
  }

  deleteOrganization(id: string): Observable<void> {
    return this.api.deleteOrganization(id).pipe(
      switchMap((org) =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.auth.refreshUserProfile();
          }),
          map(() => org),
        ),
      ),
    );
  }

  transferOrganizationOwnership(
    organizationId: string,
    newOwnerUserId: string,
  ): Observable<void> {
    return this.api
      .transferOrganizationOwnership(organizationId, { newOwnerUserId })
      .pipe(
        switchMap(() =>
          this.api.listUserOrganizations(0, 10).pipe(
            tap((page) => {
              this.organizations.set(page.items);
              this.auth.refreshUserProfile();
            }),
            map(() => undefined),
          ),
        ),
      );
  }

  addOrganizationMember(organizationId: string, memberUserId: string): Observable<void> {
    return this.api.addOrganizationMember(organizationId, memberUserId).pipe(
      switchMap(() =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.auth.refreshUserProfile();
          }),
          map(() => undefined),
        ),
      ),
    );
  }

  removeOrganizationMember(organizationId: string, memberUserId: string): Observable<void> {
    return this.api.removeOrganizationMember(organizationId, memberUserId).pipe(
      switchMap(() =>
        this.api.listUserOrganizations(0, 10).pipe(
          tap((page) => {
            this.organizations.set(page.items);
            this.auth.refreshUserProfile();
          }),
          map(() => undefined),
        ),
      ),
    );
  }

  updateOrganizationSlaPolicy(
    organizationId: string,
    body: OrganizationSlaPolicyDto,
  ): Observable<OrganizationSlaPolicyDto> {
    return this.api.updateOrganizationSlaPolicy(organizationId, body);
  }

  private messageFromHttp(err: unknown): string {
    if (!(err instanceof HttpErrorResponse)) {
      return 'Something went wrong. Please try again.';
    }
    if (err.status === 0) {
      return 'Cannot reach the server. Check your connection.';
    }
    if (err.status === 400) {
      const detail = err.error?.detail;
      return typeof detail === 'string' ? detail : 'Check the form and try again.';
    }
    if (err.status === 401) {
      return 'You are not signed in or your session expired.';
    }
    return 'Something went wrong. Please try again.';
  }
}
