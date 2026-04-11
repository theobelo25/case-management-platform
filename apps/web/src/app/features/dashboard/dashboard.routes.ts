import { Routes } from '@angular/router';
import { ProtectedLayoutComponent } from '@app/layouts/protected-layout/protected-layout.component';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    component: ProtectedLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/dashboard-home/dashboard-home.component').then(
            (m) => m.DashboardHomeComponent,
          ),
        title: 'Dashboard',
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./pages/dashboard-settings/dashboard-settings.component').then(
            (m) => m.DashboardSettingsComponent,
          ),
        title: 'Settings',
      },
      {
        path: 'cases',
        loadChildren: () => import('../cases/cases.routes').then((m) => m.CASES_ROUTES),
        title: 'Cases',
      },
      {
        path: 'organizations/manage',
        loadComponent: () =>
          import('./pages/dashboard-organization-manage/dashboard-organization-manage.component').then(
            (m) => m.DashboardOrganizationManageComponent,
          ),
        title: 'Manage organizations',
      },
      {
        path: 'organizations/:organizationId',
        loadComponent: () =>
          import('../organizations/pages/organization-detail-page/organization-detail-page.component').then(
            (m) => m.OrganizationDetailPageComponent,
          ),
        title: 'Organization details',
      },
    ],
  },
];
