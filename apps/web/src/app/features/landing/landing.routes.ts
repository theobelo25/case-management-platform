import { Routes } from '@angular/router';
import { PublicLayoutComponent } from '@app/layouts/public-layout/public-layout.component';

export const LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/home/home.component').then((m) => m.HomeComponent),
        title: 'Case Management Platform',
      },
      {
        path: 'demo',
        loadComponent: () => import('./pages/demo/demo.component').then((m) => m.DemoComponent),
        title: 'Product demo',
      },
      {
        path: '**',
        loadComponent: () =>
          import('./pages/not-found/not-found.component').then((m) => m.NotFoundComponent),
        title: 'Page not found',
      },
    ],
  },
];
