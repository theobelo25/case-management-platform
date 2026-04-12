import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-organization-confirm-delete',
  standalone: true,
  templateUrl: './organization-confirm-delete.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationConfirmDeleteComponent {
  readonly organizationName = input.required<string>();
  readonly isSubmitting = input(false);
  protected readonly typedName = signal('');
  readonly cancelled = output<void>();
  readonly confirmed = output<void>();
}
