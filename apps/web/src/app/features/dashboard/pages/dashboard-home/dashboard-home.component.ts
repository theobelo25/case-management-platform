import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  templateUrl: './dashboard-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHomeComponent {}
