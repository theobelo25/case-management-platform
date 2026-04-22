import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { AuthService } from '@app/core/auth/auth.service';
import { OrganizationMemberViewModel } from '../../models/organization-detail-view-model';

@Component({
  selector: 'app-organization-detail-lifecycle',
  templateUrl: './organization-detail-lifecycle.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationDetailLifecycleComponent {
  private readonly auth = inject(AuthService);

  readonly members = input.required<OrganizationMemberViewModel[]>();
  readonly isArchived = input(false);

  protected readonly currentUserId = computed(() => this.auth.userProfile()?.id ?? null);

  protected readonly showTransferOwnership = computed(() => {
    const uid = this.currentUserId();
    if (uid == null) return false;
    if (this.isArchived()) return false;
    return this.members().some((m) => m.id === uid && m.role === 'Owner');
  });

  protected readonly transferOwnershipDisabled = computed(() => {
    const uid = this.currentUserId();
    if (uid == null) return true;
    return !this.members().some((m) => m.id !== uid);
  });

  readonly archiveRequested = output<void>();
  readonly unarchiveRequested = output<void>();
  readonly deleteRequested = output<void>();
  readonly transferOwnershipRequested = output<void>();
}

