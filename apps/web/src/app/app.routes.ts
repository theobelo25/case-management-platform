import { Routes } from '@angular/router';
import { authGuard } from '@app/core/guards/auth.guard';

/**
 * More specific paths (`auth`, `app`) must come before `path: ''`.
 * Otherwise the lazy-loaded landing tree (which includes a `**` fallback) can consume
 * segments like `app` and show the public 404 instead of the dashboard.
 */
export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
  },
  { path: 'sign-in', pathMatch: 'full', redirectTo: '/auth/sign-in' },
  { path: 'login', pathMatch: 'full', redirectTo: '/auth/sign-in' },
  {
    path: '',
    loadChildren: () => import('./features/landing/landing.routes').then((m) => m.LANDING_ROUTES),
  },
];
