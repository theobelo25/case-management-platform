import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { OrganizationMemberViewModel } from '../../models/organization-detail-view-model';

@Component({
  selector: 'app-organization-transfer-ownership',
  standalone: true,
  templateUrl: './organization-transfer-ownership.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationTransferOwnershipComponent {
  readonly members = input.required<OrganizationMemberViewModel[]>();
  readonly currentUserId = input.required<string>();
  readonly isSubmitting = input(false);

  readonly cancelled = output<void>();
  readonly confirmed = output<string>();

  protected readonly candidates = computed(() =>
    this.members().filter((m) => m.id !== this.currentUserId()),
  );

  protected readonly selectedUserId = signal<string | null>(null);

  constructor() {
    effect(() => {
      const list = this.candidates();
      const cur = this.selectedUserId();
      if (list.length === 0) {
        this.selectedUserId.set(null);
        return;
      }
      if (cur == null || !list.some((m) => m.id === cur)) {
        this.selectedUserId.set(list[0]!.id);
      }
    });
  }
}
