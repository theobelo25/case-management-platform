import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-organization-new',
  standalone: true,
  templateUrl: './dashboard-organization-new.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardOrganizationNewComponent {}
