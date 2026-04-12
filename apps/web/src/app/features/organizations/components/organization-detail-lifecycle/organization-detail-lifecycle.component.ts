import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-organization-detail-lifecycle',
  standalone: true,
  templateUrl: './organization-detail-lifecycle.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationDetailLifecycleComponent {
  readonly isArchived = input(false);

  readonly archiveRequested = output<void>();
  readonly unarchiveRequested = output<void>();
  readonly deleteRequested = output<void>();
}
