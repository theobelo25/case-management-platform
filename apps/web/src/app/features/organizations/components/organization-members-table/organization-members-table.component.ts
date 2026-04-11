import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { OrganizationMemberViewModel } from '../../models/organization-detail.view-model';

@Component({
  selector: 'app-organization-members-table',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './organization-members-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationMembersTableComponent {
  readonly members = input.required<OrganizationMemberViewModel[]>();
}
