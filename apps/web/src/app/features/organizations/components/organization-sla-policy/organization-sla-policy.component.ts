import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { OrganizationSlaPolicyDto } from '@app/core/organizations/organizations-api.service';

@Component({
  selector: 'app-organization-sla-policy',
  imports: [FormsModule],
  templateUrl: './organization-sla-policy.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationSlaPolicyComponent {
  readonly policy = input.required<OrganizationSlaPolicyDto>();
  readonly canEdit = input(false);
  readonly isSubmitting = input(false);

  readonly saveRequested = output<OrganizationSlaPolicyDto>();

  protected readonly lowHours = signal(24);
  protected readonly mediumHours = signal(8);
  protected readonly highHours = signal(4);

  constructor() {
    effect(() => {
      const p = this.policy();
      this.lowHours.set(p.lowHours);
      this.mediumHours.set(p.mediumHours);
      this.highHours.set(p.highHours);
    });
  }

  protected setLowHours(v: unknown): void {
    this.lowHours.set(coerceHours(v));
  }

  protected setMediumHours(v: unknown): void {
    this.mediumHours.set(coerceHours(v));
  }

  protected setHighHours(v: unknown): void {
    this.highHours.set(coerceHours(v));
  }

  protected save(): void {
    this.saveRequested.emit({
      lowHours: this.lowHours(),
      mediumHours: this.mediumHours(),
      highHours: this.highHours(),
    });
  }
}

function coerceHours(v: unknown): number {
  if (typeof v === 'number' && Number.isFinite(v)) return Math.max(1, Math.min(8760, Math.trunc(v)));
  const n = Number(v);
  if (Number.isFinite(n)) return Math.max(1, Math.min(8760, Math.trunc(n)));
  return 1;
}

