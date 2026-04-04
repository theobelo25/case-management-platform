import { Routes } from '@angular/router';
import { guestGuard } from '@app/core/guards/guest.guard';
import { AuthLayoutComponent } from '@app/layouts/auth-layout/auth-layout.component';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      {
        path: 'sign-in',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./pages/sign-in/sign-in.component').then((m) => m.SignInComponent),
        title: 'Sign in',
      },
      {
        path: 'sign-up',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./pages/sign-up/sign-up.component').then((m) => m.SignUpComponent),
        title: 'Sign up',
      },
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'sign-in',
      },
    ],
  },
];
