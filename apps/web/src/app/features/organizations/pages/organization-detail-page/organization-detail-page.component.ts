import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, combineLatest, finalize, map, of, startWith, switchMap } from 'rxjs';
import { OrganizationDetailSummaryComponent } from '../../components/organization-detail-summary/organization-detail-summary.component';
import { OrganizationMembersTableComponent } from '../../components/organization-members-table/organization-members-table.component';
import {
  OrganizationDetailViewModel,
  OrganizationMemberViewModel,
} from '../../models/organization-detail-view-model';
import { AuthService } from '@app/core/auth/auth.service';
import { OrganizationsService } from '@app/core/organizations/organizations.service';
import {
  OrganizationDetailsResponseDto,
  UserMembershipResponseDto,
} from '@app/core/organizations/organizations-api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { OrganizationDetailLifecycleComponent } from '../../components/organization-detail-lifecycle/organization-detail-lifecycle.component';
import { OrganizationConfirmDeleteComponent } from '../../components/organization-confirm-delete/organization-confirm-delete.component';
import { OrganizationTransferOwnershipComponent } from '../../components/organization-transfer-ownership/organization-transfer-ownership.component';
import { OrganizationAddMemberComponent } from '../../components/organization-add-member/organization-add-member.component';

type OrganizationDetailVm =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: OrganizationDetailsResponseDto }
  | { status: 'error'; message: string };

@Component({
  selector: 'app-organization-detail-page',
  standalone: true,
  imports: [
    RouterLink,
    OrganizationDetailSummaryComponent,
    OrganizationMembersTableComponent,
    OrganizationDetailLifecycleComponent,
    OrganizationConfirmDeleteComponent,
    OrganizationTransferOwnershipComponent,
    OrganizationAddMemberComponent,
  ],
  templateUrl: './organization-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly organizations = inject(OrganizationsService);
  private readonly auth = inject(AuthService);

  protected readonly confirmingDelete = signal(false);
  protected readonly deleteError = signal<string | null>(null);
  protected readonly deleteSubmitting = signal(false);
  protected readonly confirmingTransfer = signal(false);
  protected readonly transferError = signal<string | null>(null);
  protected readonly transferSubmitting = signal(false);
  protected readonly confirmingAddMember = signal(false);
  protected readonly addMemberError = signal<string | null>(null);
  protected readonly addMemberSubmitting = signal(false);
  protected readonly removeMemberTarget = signal<{ id: string; name: string } | null>(null);
  protected readonly removeMemberError = signal<string | null>(null);
  protected readonly removeMemberSubmitting = signal(false);
  protected readonly detailVersion = signal(0);

  protected readonly currentUserId = computed(() => this.auth.userProfile()?.id ?? null);

  protected readonly organization = computed((): OrganizationDetailViewModel | null => {
    const state = this.detail();
    if (state.status !== 'success') return null;

    const org = state.data.organization;

    return {
      id: org.id,
      name: org.name,
      createdAtUtc: org.createdAtUtc,
      isArchived: org.isArchived,
    };
  });

  protected readonly members = computed((): OrganizationMemberViewModel[] => {
    const state = this.detail();
    if (state.status !== 'success') return [];
    return state.data.members.map(memberToViewModel);
  });

  protected readonly detail = toSignal(
    combineLatest([this.route.paramMap, toObservable(this.detailVersion)]).pipe(
      map(([paramMap]) => paramMap.get('organizationId')),
      switchMap((id) => {
        if (id == null || id === '') return of<OrganizationDetailVm>({ status: 'idle' });

        return this.organizations.getOrganizationDetailsReadonly(id).pipe(
          map(
            (data): OrganizationDetailVm => ({
              status: 'success',
              data,
            }),
          ),
          startWith<OrganizationDetailVm>({ status: 'loading' }),
          catchError((err: unknown) =>
            of<OrganizationDetailVm>({
              status: 'error',
              message: messageFromHttp(err),
            }),
          ),
        );
      }),
    ),
    { initialValue: { status: 'idle' } satisfies OrganizationDetailVm },
  );

  protected onArchiveRequested(): void {
    const id = this.organization()?.id;
    if (id == null) return;
    this.organizations.archiveOrganization(id).subscribe({
      next: () => this.detailVersion.update((v) => v + 1),
    });
  }

  protected onUnarchiveRequested(): void {
    const id = this.organization()?.id;
    if (id == null) return;
    this.organizations.unarchiveOrganization(id).subscribe({
      next: () => this.detailVersion.update((v) => v + 1),
    });
  }

  protected onAddMembers(): void {
    this.addMemberError.set(null);
    this.confirmingAddMember.set(true);
  }

  protected closeAddMember(): void {
    this.addMemberError.set(null);
    this.confirmingAddMember.set(false);
  }

  protected openRemoveMember(target: { id: string; name: string }): void {
    this.removeMemberError.set(null);
    this.removeMemberTarget.set(target);
  }

  protected closeRemoveMember(): void {
    this.removeMemberError.set(null);
    this.removeMemberTarget.set(null);
  }

  protected confirmRemoveMember(): void {
    const orgId = this.organization()?.id;
    const target = this.removeMemberTarget();
    if (orgId == null || target == null) return;

    this.removeMemberError.set(null);
    this.removeMemberSubmitting.set(true);
    this.organizations
      .removeOrganizationMember(orgId, target.id)
      .pipe(finalize(() => this.removeMemberSubmitting.set(false)))
      .subscribe({
        next: () => {
          const leftSelf = target.id === this.currentUserId();
          this.closeRemoveMember();
          if (leftSelf) {
            void this.router.navigateByUrl('/app/organizations/manage');
          } else {
            this.detailVersion.update((v) => v + 1);
          }
        },
        error: (err: unknown) => {
          this.removeMemberError.set(messageFromHttp(err));
        },
      });
  }

  protected onMemberSettings(_member: OrganizationMemberViewModel): void {}

  protected onAddMemberConfirmed(memberUserId: string): void {
    const id = this.organization()?.id;
    if (id == null) return;

    this.addMemberError.set(null);
    this.addMemberSubmitting.set(true);
    this.organizations
      .addOrganizationMember(id, memberUserId)
      .pipe(finalize(() => this.addMemberSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.closeAddMember();
          this.detailVersion.update((v) => v + 1);
        },
        error: (err: unknown) => {
          this.addMemberError.set(messageFromHttp(err));
        },
      });
  }

  protected openTransferOwnership(): void {
    this.transferError.set(null);
    this.confirmingTransfer.set(true);
  }

  protected closeTransferOwnership(): void {
    this.transferError.set(null);
    this.confirmingTransfer.set(false);
  }

  protected onTransferOwnershipConfirmed(newOwnerUserId: string): void {
    const id = this.organization()?.id;
    if (id == null) return;

    this.transferError.set(null);
    this.transferSubmitting.set(true);
    this.organizations
      .transferOrganizationOwnership(id, newOwnerUserId)
      .pipe(finalize(() => this.transferSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.closeTransferOwnership();
          this.detailVersion.update((v) => v + 1);
        },
        error: (err: unknown) => {
          this.transferError.set(messageFromHttp(err));
        },
      });
  }

  protected openDeleteConfirm(): void {
    this.deleteError.set(null);
    this.confirmingDelete.set(true);
  }

  protected closeDeleteConfirm(): void {
    this.deleteError.set(null);
    this.confirmingDelete.set(false);
  }

  protected onDeleteConfirmed(): void {
    const id = this.organization()?.id;
    if (id == null) return;

    this.deleteError.set(null);
    this.deleteSubmitting.set(true);
    this.organizations
      .deleteOrganization(id)
      .pipe(finalize(() => this.deleteSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.closeDeleteConfirm();
          void this.router.navigateByUrl('/app/organizations/manage');
        },
        error: (err: unknown) => {
          this.deleteError.set(messageFromHttp(err));
        },
      });
  }
}

function messageFromHttp(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const detail = err.error?.detail;
    if (typeof detail === 'string') return detail;
    return `Request failed (${err.status}).`;
  }
  return 'Something went wrong.';
}

function memberToViewModel(m: UserMembershipResponseDto): OrganizationMemberViewModel {
  return {
    id: m.id,
    name: m.name,
    role: m.role,
    email: m.email,
    joinedAtUtc: m.joinedAtUtc,
  };
}
