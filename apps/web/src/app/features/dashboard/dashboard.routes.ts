import { Routes } from '@angular/router';
import { ProtectedLayoutComponent } from '../../layouts/protected-layout/protected-layout.component';

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
    ],
  },
];
