import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CreateOrganizationFormComponent } from '@app/features/organizations/components/create-organization-form/create-organization-form.component';
import { OrganizationsListComponent } from '@app/features/organizations/components/organizations-list/organizations-list.component';

@Component({
  selector: 'app-dashboard-organization-manage',
  standalone: true,
  imports: [CreateOrganizationFormComponent, OrganizationsListComponent],
  templateUrl: './dashboard-organization-manage.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardOrganizationManageComponent {}
