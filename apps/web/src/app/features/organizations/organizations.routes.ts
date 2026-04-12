import { Routes } from '@angular/router';

export const ORGANIZATIONS_ROUTES: Routes = [
  {
    path: 'manage',
    loadComponent: () =>
      import('../dashboard/pages/dashboard-organization-manage/dashboard-organization-manage.component').then(
        (m) => m.DashboardOrganizationManageComponent,
      ),
    title: 'Manage organizations',
  },
  {
    path: ':organizationId',
    loadComponent: () =>
      import('./pages/organization-detail-page/organization-detail-page.component').then(
        (m) => m.OrganizationDetailPageComponent,
      ),
    title: 'Organization details',
  },
];
