import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'analytics', pathMatch: 'full' },
  {
    path: 'analytics',
    loadComponent: () => import('./features/analytics/analytics-list/analytics-list.component')
      .then(m => m.AnalyticsListComponent)
  },
  {
    path: 'analytics/new',
    loadComponent: () => import('./features/analytics/analytics-form/analytics-form.component')
      .then(m => m.AnalyticsFormComponent)
  },
  {
    path: 'analytics/:id',
    loadComponent: () => import('./features/analytics/analytics-detail/analytics-detail.component')
      .then(m => m.AnalyticsDetailComponent)
  },
  {
    path: 'analytics/:id/edit',
    loadComponent: () => import('./features/analytics/analytics-form/analytics-form.component')
      .then(m => m.AnalyticsFormComponent)
  },
  {
    path: 'analytics/:id/runs',
    loadComponent: () => import('./features/analytics-run/analytics-run-list/analytics-run-list.component')
      .then(m => m.AnalyticsRunListComponent)
  },
  {
    path: 'review',
    loadComponent: () => import('./features/review/review-list/review-list.component')
      .then(m => m.ReviewListComponent)
  },
  {
    path: 'chat',
    loadComponent: () => import('./features/chat/chat.component')
      .then(m => m.ChatComponent)
  },
  { path: '**', redirectTo: 'analytics' }
];
