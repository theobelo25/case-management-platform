import { Routes } from '@angular/router';
import { AuthLayoutComponent } from '../../layouts/auth-layout/auth-layout.component';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      {
        path: 'sign-in',
        loadComponent: () => import('./pages/sign-in/sign-in.component').then((m) => m.SignInComponent),
        title: 'Sign in',
      },
      {
        path: 'sign-up',
        loadComponent: () => import('./pages/sign-up/sign-up.component').then((m) => m.SignUpComponent),
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
