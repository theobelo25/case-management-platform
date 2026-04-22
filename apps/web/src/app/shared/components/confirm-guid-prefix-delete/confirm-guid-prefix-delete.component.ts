import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { guidPrefixForConfirmation } from '../../utils/guid-prefix-for-confirmation';

@Component({
  selector: 'app-confirm-guid-prefix-delete',
  templateUrl: './confirm-guid-prefix-delete.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmGuidPrefixDeleteComponent {
  readonly headline = input.required<string>();
  readonly resourceId = input.required<string>();
  readonly isSubmitting = input(false);
  protected readonly typedValue = signal('');
  protected readonly expectedPrefix = computed(() => guidPrefixForConfirmation(this.resourceId()));
  readonly cancelled = output<void>();
  readonly confirmed = output<void>();

  protected readonly canConfirm = computed(() => {
    const typed = this.typedValue().trim().toLowerCase();
    return typed.length > 0 && typed === this.expectedPrefix();
  });
}

