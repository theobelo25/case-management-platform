import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-how-it-works',
  standalone: true,
  templateUrl: './how-it-works.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HowItWorksComponent {
  protected readonly steps = [
    'Sign in securely to access your assigned work.',
    'Browse open cases and filter by status, priority, or assignee.',
    'Open a case to review details, timeline history, and next actions.',
  ];
}
