import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AboutComponent {
  protected readonly highlights = [
    'Production-style Angular architecture with feature-based routing and reusable shell layouts.',
    'ASP.NET backend organized with layered boundaries (API, application, and infrastructure concerns).',
    'JWT authentication flow with guarded routes and role-aware access patterns.',
    'Real-time updates support through SignalR-style hub integration for live notifications.',
    'Interactive dashboard experience with chart-driven insights and profile/settings workflows.',
    'Container-ready setup (Docker + compose) for local development and deployment portability.',
  ];
}
