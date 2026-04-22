import { DatePipe, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CasesService } from '@app/core/cases/cases.service';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, finalize, map, of, startWith, switchMap } from 'rxjs';
import {
  CASE_PRIORITIES,
  CASE_STATUSES,
  CaseItem,
  CasePriority,
  CaseStatus,
  CaseTimelineItem,
} from '../../models/cases.types';
import { CaseAssignModalComponent } from '../../components/case-assign-modal/case-assign-modal.component';
import { ConfirmGuidPrefixDeleteComponent } from '@app/shared/components/confirm-guid-prefix-delete/confirm-guid-prefix-delete.component';
import { UserSearchHitDto } from '@app/core/users/users-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { OrganizationsApiService } from '@app/core/organizations/organizations-api.service';
import {
  getSlaBadgeTone,
  getSlaDetailParts,
  slaBadgeToneClasses,
} from '../../models/sla-derived-label';

type CaseDetailVm =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: CaseItem }
  | { status: 'error'; message: string };

@Component({
  selector: 'app-case-detail',
  imports: [RouterLink, CaseAssignModalComponent, ConfirmGuidPrefixDeleteComponent, DatePipe, NgClass],
  templateUrl: './case-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly casesService = inject(CasesService);
  private readonly auth = inject(AuthService);
  private readonly organizationsApi = inject(OrganizationsApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly detail = toSignal(
    this.route.paramMap.pipe(
      map((params) => params.get('caseId')),
      switchMap((caseId) => {
        if (caseId == null || caseId === '') {
          return of<CaseDetailVm>({ status: 'idle' });
        }

        return this.casesService.getCase(caseId).pipe(
          map((data): CaseDetailVm => ({ status: 'success', data })),
          startWith<CaseDetailVm>({ status: 'loading' }),
          catchError((err: unknown) =>
            of<CaseDetailVm>({
              status: 'error',
              message: this.getCaseLoadErrorMessage(err),
            }),
          ),
        );
      }),
    ),
    { initialValue: { status: 'idle' } satisfies CaseDetailVm },
  );

  protected readonly caseItem = computed(() => {
    const vm = this.detail();
    if (vm.status !== 'success') return null;
    return vm.data;
  });

  protected readonly statusOptions = CASE_STATUSES;
  protected readonly priorityOptions = CASE_PRIORITIES;

  protected readonly currentCase = signal<CaseItem | null>(null);
  protected readonly selectedStatus = signal<CaseStatus>('Open');
  protected readonly selectedPriority = signal<CasePriority>('Low');
  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly assignNotice = signal<string | null>(null);
  protected readonly assignModalOpen = signal(false);
  protected readonly assignSearchLoading = signal(false);
  protected readonly isAssigning = signal(false);
  protected readonly assignSearchError = signal<string | null>(null);
  protected readonly orgMemberSearchUsers = signal<UserSearchHitDto[]>([]);
  protected readonly messageDraft = signal('');
  protected readonly messageIsInternal = signal(false);
  protected readonly messageNotice = signal<string | null>(null);
  protected readonly isPostingMessage = signal(false);
  protected readonly dangerZoneError = signal<string | null>(null);
  protected readonly dangerZoneBusy = signal(false);
  protected readonly confirmingDelete = signal(false);
  protected readonly deleteSubmitting = signal(false);
  protected readonly confirmingArchive = signal(false);
  protected readonly archiveSubmitting = signal(false);

  protected readonly displayCase = computed(() => this.currentCase() ?? this.caseItem());
  protected readonly slaUi = computed(() => {
    const c = this.displayCase();
    if (!c) {
      return null;
    }
    const tone = getSlaBadgeTone(c);
    return {
      parts: getSlaDetailParts(c),
      badgeClass: slaBadgeToneClasses[tone],
    };
  });
  protected readonly timelineItems = computed(() => this.displayCase()?.timeline ?? []);
  protected readonly caseIsArchived = computed(() => this.displayCase()?.isArchived === true);
  protected readonly canShowCaseDangerZone = computed(() => {
    const c = this.displayCase();
    if (!c?.organizationId) {
      return false;
    }
    return this.auth.canManageCasesForOrganization(c.organizationId);
  });

  constructor() {
    effect(() => {
      const item = this.caseItem();
      if (!item) return;

      this.currentCase.set(item);
      this.selectedStatus.set(item.status);
      this.selectedPriority.set(item.priority);
    });
  }

  protected onStatusChange(value: string): void {
    if (!this.isCaseStatus(value)) return;
    const previousStatus = this.selectedStatus();
    const previousPriority = this.selectedPriority();
    this.selectedStatus.set(value);
    this.persistCaseMetadata(previousStatus, previousPriority);
  }

  protected onPriorityChange(value: string): void {
    if (!this.isCasePriority(value)) return;
    const previousStatus = this.selectedStatus();
    const previousPriority = this.selectedPriority();
    this.selectedPriority.set(value);
    this.persistCaseMetadata(previousStatus, previousPriority);
  }

  protected openArchiveConfirm(): void {
    const current = this.displayCase();
    if (!current || this.caseIsArchived()) {
      return;
    }
    this.dangerZoneError.set(null);
    this.confirmingArchive.set(true);
  }

  protected closeArchiveConfirm(): void {
    this.dangerZoneError.set(null);
    this.confirmingArchive.set(false);
  }

  protected onArchiveConfirmed(): void {
    const current = this.displayCase();
    if (!current || this.caseIsArchived()) {
      this.closeArchiveConfirm();
      return;
    }
    this.dangerZoneError.set(null);
    this.archiveSubmitting.set(true);
    this.casesService
      .archiveCase(current.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.archiveSubmitting.set(false)),
      )
      .subscribe({
        next: (updated) => {
          this.currentCase.set(updated);
          this.closeArchiveConfirm();
        },
        error: (err: unknown) => {
          this.dangerZoneError.set(this.getDangerZoneErrorMessage(err));
        },
      });
  }

  protected onUnarchiveCase(): void {
    const current = this.displayCase();
    if (!current || !this.caseIsArchived()) {
      return;
    }
    this.dangerZoneError.set(null);
    this.dangerZoneBusy.set(true);
    this.casesService
      .unarchiveCase(current.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.dangerZoneBusy.set(false)),
      )
      .subscribe({
        next: (updated) => {
          this.currentCase.set(updated);
        },
        error: (err: unknown) => {
          this.dangerZoneError.set(this.getDangerZoneErrorMessage(err));
        },
      });
  }

  protected openDeleteConfirm(): void {
    this.dangerZoneError.set(null);
    this.confirmingDelete.set(true);
  }

  protected closeDeleteConfirm(): void {
    this.dangerZoneError.set(null);
    this.confirmingDelete.set(false);
  }

  protected onDeleteConfirmed(): void {
    const current = this.displayCase();
    if (!current) {
      this.closeDeleteConfirm();
      return;
    }
    this.dangerZoneError.set(null);
    this.deleteSubmitting.set(true);
    this.casesService
      .deleteCase(current.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.deleteSubmitting.set(false)),
      )
      .subscribe({
        next: () => {
          this.closeDeleteConfirm();
          void this.router.navigate(['/app', 'cases']);
        },
        error: (err: unknown) => {
          this.dangerZoneError.set(this.getDangerZoneErrorMessage(err));
        },
      });
  }

  protected onAssignTask(): void {
    if (this.caseIsArchived()) {
      return;
    }
    this.assignNotice.set(null);
    const organizationId = this.auth.getEffectiveActiveOrganizationId();
    if (!organizationId) {
      this.assignSearchError.set('No organization available for assignment.');
      return;
    }

    this.assignSearchError.set(null);
    this.assignSearchLoading.set(true);
    this.organizationsApi
      .getOrganizationDetails(organizationId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.assignSearchLoading.set(false)),
      )
      .subscribe({
        next: (detail) => {
          this.orgMemberSearchUsers.set(
            detail.members.map((m) => ({
              userId: m.id,
              fullName: m.name,
              email: m.email,
            })),
          );
          this.assignModalOpen.set(true);
        },
        error: () => {
          this.assignSearchError.set('Could not load organization members.');
        },
      });
  }

  protected onAssignModalClosed(): void {
    this.assignModalOpen.set(false);
  }

  protected onAssigneeSelected(user: UserSearchHitDto): void {
    const current = this.displayCase();
    if (!current) return;

    this.isAssigning.set(true);
    this.assignSearchError.set(null);
    this.casesService
      .assignCase({
        caseId: current.id,
        assigneeUserId: user.userId,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isAssigning.set(false)),
      )
      .subscribe({
        next: (updatedCase) => {
          this.currentCase.set(updatedCase);
          this.assignModalOpen.set(false);
          this.assignNotice.set(`Assigned to ${user.fullName}.`);
        },
        error: (err: unknown) => {
          this.assignSearchError.set(this.getAssignErrorMessage(err));
        },
      });
  }

  protected onUnassign(): void {
    const current = this.displayCase();
    if (
      !current ||
      current.assigneeUserId == null ||
      current.status === 'Closed' ||
      this.caseIsArchived()
    ) {
      return;
    }

    this.isAssigning.set(true);
    this.assignSearchError.set(null);
    this.casesService
      .assignCase({
        caseId: current.id,
        assigneeUserId: null,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isAssigning.set(false)),
      )
      .subscribe({
        next: (updatedCase) => {
          this.currentCase.set(updatedCase);
          this.assignNotice.set('Case unassigned.');
        },
        error: (err: unknown) => {
          this.assignSearchError.set(this.getAssignErrorMessage(err));
        },
      });
  }

  protected timelineItemLabel(item: CaseTimelineItem): string {
    if (item.type === 'event') {
      const et = item.eventType;
      if (et === 'sla_due_changed') return 'SLA due changed';
      if (et === 'sla_breached') return 'SLA breached';
      return et?.replace(/_/g, ' ') ?? 'Event';
    }
    return item.isInternal ? 'Internal note' : item.isInitial ? 'Initial request' : 'Message';
  }

  protected updateMessageDraft(value: string): void {
    this.messageDraft.set(value);
  }

  protected toggleInternalMessage(value: boolean): void {
    this.messageIsInternal.set(value);
  }

  protected submitMessage(): void {
    const item = this.displayCase();
    if (!item || this.caseIsArchived()) {
      return;
    }

    const body = this.messageDraft().trim();
    if (!body) {
      this.messageNotice.set('Please enter a message before posting.');
      return;
    }

    this.messageNotice.set(null);
    this.isPostingMessage.set(true);
    this.casesService
      .addCaseComment({
        caseId: item.id,
        body,
        isInternal: this.messageIsInternal(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isPostingMessage.set(false)),
      )
      .subscribe({
        next: (updatedCase) => {
          this.currentCase.set(updatedCase);
          this.messageDraft.set('');
          this.messageIsInternal.set(false);
          this.messageNotice.set('Message posted.');
        },
        error: (err: unknown) => {
          this.messageNotice.set(this.getMessagePostError(err));
        },
      });
  }

  private isCaseStatus(value: string): value is CaseStatus {
    return (this.statusOptions as readonly string[]).includes(value);
  }

  private isCasePriority(value: string): value is CasePriority {
    return (this.priorityOptions as readonly string[]).includes(value);
  }

  private persistCaseMetadata(previousStatus: CaseStatus, previousPriority: CasePriority): void {
    const item = this.caseItem();
    if (!item || item.isArchived) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.casesService
      .updateCaseMetadata({
        caseId: item.id,
        status: this.selectedStatus(),
        priority: this.selectedPriority(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedCase) => {
          this.currentCase.set(updatedCase);
          this.selectedStatus.set(updatedCase.status);
          this.selectedPriority.set(updatedCase.priority);
          this.isSaving.set(false);
        },
        error: (err: unknown) => {
          this.selectedStatus.set(previousStatus);
          this.selectedPriority.set(previousPriority);
          this.isSaving.set(false);
          this.saveError.set(this.getSaveErrorMessage(err));
        },
      });
  }

  private getCaseLoadErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 400 || err.status === 422) {
        const detail = this.tryGetValidationMessage(err.error);
        return detail ?? 'Could not load case details: invalid request.';
      }

      if (err.status === 401) {
        return 'Your session expired. Please sign in again.';
      }

      if (err.status === 403) {
        return 'You do not have permission to view this case.';
      }

      if (err.status === 404) {
        return 'Case not found.';
      }

      if (err.status === 0) {
        return 'Cannot reach the server. Check your connection.';
      }

      const detail = this.tryGetValidationMessage(err.error);
      if (detail) {
        return detail;
      }
    }

    return 'Could not load case details. Please try again.';
  }

  private getSaveErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 400) {
        const detail = this.tryGetValidationMessage(err.error);
        return detail ?? 'Could not save case status/priority: invalid request.';
      }

      if (err.status === 401) {
        return 'Your session expired. Please sign in again.';
      }

      if (err.status === 403) {
        return 'You do not have permission to update this case.';
      }

      if (err.status === 404) {
        return 'Case not found.';
      }

      if (err.status === 0) {
        return 'Cannot reach the server. Check your connection.';
      }
    }

    return 'Could not save case status/priority. Please try again.';
  }

  private getMessagePostError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 400) {
        const detail = this.tryGetValidationMessage(err.error);
        return detail ?? 'Message could not be posted. Please check the content.';
      }
      if (err.status === 401) return 'Your session expired. Please sign in again.';
      if (err.status === 403) return 'You do not have permission to post to this case.';
      if (err.status === 404) return 'Case not found.';
      if (err.status === 0) return 'Cannot reach the server. Check your connection.';
    }
    return 'Could not post message. Please try again.';
  }

  private getDangerZoneErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const detail = this.tryGetValidationMessage(err.error);
      if (err.status === 401) {
        return 'Your session expired. Please sign in again.';
      }
      if (err.status === 403) {
        return detail ?? 'You do not have permission to perform this action.';
      }
      if (err.status === 404) {
        return detail ?? 'Case not found.';
      }
      if (err.status === 0) {
        return 'Cannot reach the server. Check your connection.';
      }
      if (detail) {
        return detail;
      }
    }
    return 'Something went wrong. Please try again.';
  }

  private getAssignErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const detail = this.tryGetValidationMessage(err.error);

      if (err.status === 400) {
        return detail ?? 'Assignment request is invalid.';
      }
      if (err.status === 401) return 'Your session expired. Please sign in again.';
      if (err.status === 403) return 'You do not have permission to assign this case.';
      if (err.status === 404) {
        if (detail) return detail;
        return 'Assignment endpoint not available yet. Restart API to load latest changes.';
      }
      if (err.status === 0) return 'Cannot reach the server. Check your connection.';
    }
    return 'Could not assign case. Please try again.';
  }

  private tryGetValidationMessage(payload: unknown): string | null {
    if (!payload || typeof payload !== 'object') return null;
    const maybe = payload as {
      detail?: unknown;
      errors?: Record<string, unknown>;
    };

    if (typeof maybe.detail === 'string' && maybe.detail.trim() !== '') {
      return maybe.detail;
    }

    if (maybe.errors && typeof maybe.errors === 'object') {
      const first = Object.values(maybe.errors).find(
        (v) => Array.isArray(v) && v.length > 0 && typeof v[0] === 'string',
      ) as string[] | undefined;
      if (first?.[0]) return first[0];
    }

    return null;
  }
}

