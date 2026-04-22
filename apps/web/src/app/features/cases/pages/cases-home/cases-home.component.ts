import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CaseFiltersComponent } from '../../components/case-filters/case-filters.component';
import { CaseItem, CasePriority, CaseStatus } from '../../models/cases.types';
import { CaseTableComponent } from '../../components/case-table/case-table.component';
import { CaseSummaryCardsComponent } from '../../components/case-summary-cards/case-summary-cards.component';
import { CaseDirectoryChartsComponent } from '../../components/case-directory-charts/case-directory-charts.component';
import { CasesService } from '@app/core/cases/cases.service';
import { BulkCaseActionApi, CaseListQueryParams } from '@app/core/cases/cases-api.service';
import { priorityToApiCode, statusToApiCode } from '../../models/case-code-maps';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CaseAssignModalComponent } from '../../components/case-assign-modal/case-assign-modal.component';
import { OrganizationsApiService } from '@app/core/organizations/organizations-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { UserSearchHitDto } from '@app/core/users/users-api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, finalize, of } from 'rxjs';
import { PaginationControlsComponent } from '@app/shared/components/pagination-controls/pagination-controls.component';
import { SectionCardComponent } from '@app/shared/components/section-card/section-card.component';

@Component({
  selector: 'app-cases-home',
  imports: [
    RouterLink,
    CaseFiltersComponent,
    CaseTableComponent,
    CaseSummaryCardsComponent,
    CaseDirectoryChartsComponent,
    CaseAssignModalComponent,
    PaginationControlsComponent,
    SectionCardComponent,
  ],
  templateUrl: './cases-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CasesHomeComponent {
  private static readonly FIRST_PAGE_CURSOR_SENTINEL = '__FIRST_PAGE__';

  constructor() {
    effect(() => {
      this.searchTerm();
      this.pageSize();
      this.selectedStatus();
      this.selectedPriority();
      this.sortBy();
      this.overdueOnly();
      this.assignedToMe();
      this.breachedOnly();
      this.unassignedOnly();
      this.dueSoonWithinHours();
      this.apiSortDescending();
      this.currentCursor();
      this.listVersion();

      this.loading.set(true);
      this.loadError.set(null);

      this.casesService
        .listCases(this.buildListQueryParams())
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (result) => {
            this.cases.set(result.items);
            this.nextCursor.set(result.nextCursor);
            this.selectedCaseIds.set([]);
            this.loading.set(false);
          },
          error: () => {
            this.loadError.set('Could not load cases. Try again.');
            this.nextCursor.set(null);
            this.loading.set(false);
          },
        });
    });
  }

  private readonly casesService = inject(CasesService);
  private readonly organizationsApi = inject(OrganizationsApiService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly cases = signal<CaseItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly searchTerm = signal('');
  protected readonly pageSize = signal<5 | 10 | 25>(10);
  protected readonly selectedStatus = signal<'All' | CaseStatus>('All');
  protected readonly selectedPriority = signal<'All' | CasePriority>('All');
  protected readonly sortBy = signal<'updatedAt' | 'priority' | 'slaDue'>('updatedAt');
  protected readonly overdueOnly = signal(false);

  protected readonly assignedToMe = signal(false);
  protected readonly breachedOnly = signal(false);
  protected readonly unassignedOnly = signal(false);
  protected readonly dueSoonWithinHours = signal<number | undefined>(undefined);
  /** Used with `updatedAt` / `slaDue` when API should return newest first. */
  protected readonly apiSortDescending = signal(false);

  protected readonly listVersion = signal(0);
  protected readonly listHint = signal('All cases in your organization.');
  protected readonly selectedCaseIds = signal<string[]>([]);
  protected readonly currentCursor = signal<string | undefined>(undefined);
  protected readonly nextCursor = signal<string | null>(null);
  protected readonly cursorHistory = signal<(string | null)[]>([]);
  protected readonly pageNumber = signal(1);
  protected readonly bulkBusy = signal(false);
  protected readonly bulkError = signal<string | null>(null);
  protected readonly assignModalOpen = signal(false);
  protected readonly assignSearchLoading = signal(false);
  protected readonly assignSearchError = signal<string | null>(null);
  protected readonly orgMemberSearchUsers = signal<UserSearchHitDto[]>([]);

  protected readonly visibleCases = computed(() => this.cases());
  protected readonly canGoToPreviousPage = computed(() => this.pageNumber() > 1 && !this.loading());
  protected readonly canGoToNextPage = computed(() => this.nextCursor() != null && !this.loading());

  private buildListQueryParams(): CaseListQueryParams {
    const search = this.searchTerm().trim();
    const status = this.selectedStatus();
    const priority = this.selectedPriority();
    const sort = this.sortBy();

    const sortApi =
      sort === 'updatedAt' ? 'UPDATED_AT' : sort === 'priority' ? 'PRIORITY' : 'SLA_DUE';

    const sortDescending = this.resolveSortDescending(sort);

    return {
      limit: this.pageSize(),
      cursor: this.currentCursor(),
      search: search.length > 0 ? search : undefined,
      status: status === 'All' ? undefined : statusToApiCode(status),
      priority: priority === 'All' ? undefined : priorityToApiCode(priority),
      sort: sortApi,
      sortDescending,
      overdueOnly: this.overdueOnly() ? true : undefined,
      assignedToMe: this.assignedToMe() ? true : undefined,
      breachedOnly: this.breachedOnly() ? true : undefined,
      unassignedOnly: this.unassignedOnly() ? true : undefined,
      dueSoonWithinHours: this.dueSoonWithinHours(),
    };
  }

  private resolveSortDescending(sort: 'updatedAt' | 'priority' | 'slaDue'): boolean | undefined {
    if (sort === 'priority') {
      return true;
    }
    return this.apiSortDescending() ? true : undefined;
  }

  protected applyViewPreset(
    preset: 'all' | 'myDueSoon' | 'breached' | 'unassignedHigh',
  ): void {
    this.bulkError.set(null);
    this.resetPagination();
    switch (preset) {
      case 'all':
        this.assignedToMe.set(false);
        this.breachedOnly.set(false);
        this.unassignedOnly.set(false);
        this.dueSoonWithinHours.set(undefined);
        this.overdueOnly.set(false);
        this.selectedStatus.set('All');
        this.selectedPriority.set('All');
        this.sortBy.set('updatedAt');
        this.apiSortDescending.set(false);
        this.listHint.set('All cases in your organization.');
        break;
      case 'myDueSoon':
        this.assignedToMe.set(true);
        this.breachedOnly.set(false);
        this.unassignedOnly.set(false);
        this.dueSoonWithinHours.set(48);
        this.overdueOnly.set(false);
        this.selectedStatus.set('All');
        this.selectedPriority.set('All');
        this.sortBy.set('slaDue');
        this.apiSortDescending.set(false);
        this.listHint.set('Assigned to you with SLA due in the next 48 hours.');
        break;
      case 'breached':
        this.assignedToMe.set(false);
        this.breachedOnly.set(true);
        this.unassignedOnly.set(false);
        this.dueSoonWithinHours.set(undefined);
        this.overdueOnly.set(false);
        this.selectedStatus.set('All');
        this.selectedPriority.set('All');
        this.sortBy.set('updatedAt');
        this.apiSortDescending.set(true);
        this.listHint.set('Cases with a recorded SLA breach.');
        break;
      case 'unassignedHigh':
        this.assignedToMe.set(false);
        this.breachedOnly.set(false);
        this.unassignedOnly.set(true);
        this.dueSoonWithinHours.set(undefined);
        this.overdueOnly.set(false);
        this.selectedStatus.set('All');
        this.selectedPriority.set('High');
        this.sortBy.set('updatedAt');
        this.apiSortDescending.set(true);
        this.listHint.set('High priority cases with no assignee.');
        break;
    }
  }

  protected onSelectionChange(ids: string[]): void {
    this.selectedCaseIds.set(ids);
    this.bulkError.set(null);
  }

  protected readonly selectedCount = computed(() => this.selectedCaseIds().length);

  protected openBulkAssignModal(): void {
    if (this.selectedCaseIds().length === 0) {
      return;
    }
    this.assignSearchError.set(null);
    const organizationId = this.auth.getEffectiveActiveOrganizationId();
    if (!organizationId) {
      this.assignSearchError.set('No organization available for assignment.');
      return;
    }
    this.assignSearchLoading.set(true);
    this.organizationsApi
      .getOrganizationDetails(organizationId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.assignSearchLoading.set(false)),
        catchError(() => {
          this.assignSearchError.set('Could not load organization members.');
          return of(null);
        }),
      )
      .subscribe((detail) => {
        if (!detail) {
          return;
        }
        this.orgMemberSearchUsers.set(
          detail.members.map((m) => ({
            userId: m.id,
            fullName: m.name,
            email: m.email,
          })),
        );
        this.assignModalOpen.set(true);
      });
  }

  protected onAssignModalClosed(): void {
    this.assignModalOpen.set(false);
  }

  protected onBulkAssigneeSelected(user: UserSearchHitDto): void {
    this.runBulk('ASSIGN', { assigneeUserId: user.userId });
    this.assignModalOpen.set(false);
  }

  protected bulkUnassign(): void {
    this.runBulk('ASSIGN', { assigneeUserId: null });
  }

  protected onBulkPriorityChange(value: string): void {
    const p = value.trim().toUpperCase();
    if (p !== 'LOW' && p !== 'MEDIUM' && p !== 'HIGH') {
      return;
    }
    this.runBulk('SET_PRIORITY', { priority: p });
  }

  protected onBulkStatusChange(value: string): void {
    const s = value.trim().toUpperCase();
    if (!['NEW', 'OPEN', 'PENDING', 'RESOLVED', 'CLOSED'].includes(s)) {
      return;
    }
    this.runBulk('SET_STATUS', { status: s });
  }

  protected bulkBumpPriority(): void {
    this.runBulk('BUMP_PRIORITY');
  }

  private runBulk(action: BulkCaseActionApi, extra: { assigneeUserId?: string | null; priority?: string; status?: string } = {}): void {
    const ids = this.selectedCaseIds();
    if (ids.length === 0) {
      return;
    }
    this.bulkBusy.set(true);
    this.bulkError.set(null);
    this.casesService
      .bulkCases({
        caseIds: ids,
        action,
        ...extra,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.bulkBusy.set(false)),
      )
      .subscribe({
        next: () => {
          this.listVersion.update((v) => v + 1);
        },
        error: (err: unknown) => {
          this.bulkError.set(this.getBulkErrorMessage(err));
        },
      });
  }

  private getBulkErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const detail = typeof err.error?.detail === 'string' ? err.error.detail : null;
      if (detail) {
        return detail;
      }
      if (err.status === 400) {
        return 'Bulk update could not be completed. Check the selected cases.';
      }
      if (err.status === 401) {
        return 'Your session expired. Please sign in again.';
      }
      if (err.status === 403) {
        return 'You do not have permission to update these cases.';
      }
      if (err.status === 0) {
        return 'Cannot reach the server. Check your connection.';
      }
    }
    return 'Bulk update failed. Please try again.';
  }

  protected updateSearchTerm(value: string): void {
    this.resetPagination();
    this.searchTerm.set(value);
  }

  protected updatePageSize(value: string): void {
    const parsed = Number(value);
    if (parsed === 5 || parsed === 10 || parsed === 25) {
      this.resetPagination();
      this.pageSize.set(parsed);
    }
  }

  protected updateSelectedStatus(value: 'All' | CaseStatus): void {
    this.resetPagination();
    this.selectedStatus.set(value);
  }

  protected updateSelectedPriority(value: 'All' | CasePriority): void {
    this.resetPagination();
    this.selectedPriority.set(value);
  }

  protected updateSortBy(value: 'updatedAt' | 'priority' | 'slaDue'): void {
    this.resetPagination();
    this.sortBy.set(value);
  }

  protected updateOverdueOnly(value: boolean): void {
    this.resetPagination();
    this.overdueOnly.set(value);
  }

  protected goToNextPage(): void {
    const next = this.nextCursor();
    if (!next || this.loading()) {
      return;
    }

    const current = this.currentCursor() ?? CasesHomeComponent.FIRST_PAGE_CURSOR_SENTINEL;
    this.cursorHistory.update((history) => [...history, current]);
    this.currentCursor.set(next);
    this.pageNumber.update((page) => page + 1);
  }

  protected goToPreviousPage(): void {
    if (this.loading()) {
      return;
    }

    const history = this.cursorHistory();
    if (history.length === 0) {
      return;
    }

    const previous = history[history.length - 1];
    this.cursorHistory.set(history.slice(0, -1));
    const nextCurrentCursor =
      previous == null || previous === CasesHomeComponent.FIRST_PAGE_CURSOR_SENTINEL
        ? undefined
        : previous;
    this.currentCursor.set(nextCurrentCursor);
    this.pageNumber.update((page) => Math.max(1, page - 1));
  }

  private resetPagination(): void {
    this.currentCursor.set(undefined);
    this.nextCursor.set(null);
    this.cursorHistory.set([]);
    this.pageNumber.set(1);
  }

  protected readonly openCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'Open').length,
  );

  protected readonly totalCasesCount = computed(() => this.cases().length);

  protected readonly highPriorityCount = computed(
    () => this.cases().filter((caseItem) => caseItem.priority === 'High').length,
  );

  protected readonly pendingCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'Pending').length,
  );

  protected readonly closedCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'Closed').length,
  );

  protected readonly summaryCards = computed(() => [
    {
      label: 'Total Cases',
      value: this.totalCasesCount(),
      helperText: 'All cases in the system',
    },
    {
      label: 'Open Cases',
      value: this.openCasesCount(),
      helperText: 'Require attention',
    },
    {
      label: 'Pending Cases',
      value: this.pendingCasesCount(),
      helperText: 'Awaiting next action',
    },
    {
      label: 'High Priority',
      value: this.highPriorityCount(),
      helperText: 'Need urgent review',
    },
    {
      label: 'Closed Cases',
      value: this.closedCasesCount(),
      helperText: 'Completed items',
    },
  ]);
}

