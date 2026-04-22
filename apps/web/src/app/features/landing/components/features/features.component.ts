import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeaturesComponent {
  protected readonly features = [
    {
      title: 'Centralized case tracking',
      description: 'View, organize, and manage active cases from a single dashboard.',
    },
    {
      title: 'Role-based access',
      description: 'Support secure workflows for staff, supervisors, and administrators.',
    },
    {
      title: 'Case timelines',
      description: 'Follow activity, status updates, and important case events over time.',
    },
    {
      title: 'Scalable architecture',
      description: 'Built with Angular and ASP.NET to reflect modern enterprise development.',
    },
  ];
}

