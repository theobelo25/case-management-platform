import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { OrganizationMemberViewModel } from '../../models/organization-detail-view-model';

@Component({
  selector: 'app-organization-members-table',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './organization-members-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationMembersTableComponent {
  readonly members = input.required<OrganizationMemberViewModel[]>();
  readonly currentUserId = input<string | null>(null);

  readonly addMembersRequested = output<void>();
  readonly removeMemberRequested = output<{ id: string; name: string }>();
  readonly memberSettingsRequested = output<OrganizationMemberViewModel>();

  protected canOfferRemove(member: OrganizationMemberViewModel): boolean {
    const uid = this.currentUserId();
    if (uid == null) return false;
    const me = this.members().find((m) => m.id === uid);
    if (me == null) return false;
    if (member.role === 'Owner') return false;
    if (member.id === uid) return true;
    return me.role === 'Owner' || me.role === 'Admin';
  }
}
