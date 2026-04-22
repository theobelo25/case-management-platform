import { Routes } from '@angular/router';

export const CASES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/cases-home/cases-home.component').then((m) => m.CasesHomeComponent),
    title: 'Cases',
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./pages/cases-new/cases-new.component').then((m) => m.CasesNewComponent),
    title: 'New Case',
  },
  {
    path: ':caseId',
    loadComponent: () =>
      import('./pages/case-detail/case-detail.component').then((m) => m.CaseDetailComponent),
    title: 'Case Details',
  },
];
