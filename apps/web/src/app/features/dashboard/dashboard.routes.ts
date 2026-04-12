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
        path: 'organizations',
        loadChildren: () =>
          import('../organizations/organizations.routes').then((m) => m.ORGANIZATIONS_ROUTES),
      },
    ],
  },
];
