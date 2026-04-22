import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrganizationsService } from '@app/core/organizations/organizations.service';

@Component({
  selector: 'app-create-organization-form',
  imports: [ReactiveFormsModule],
  templateUrl: './create-organization-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateOrganizationFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly organizations = inject(OrganizationsService);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
  });

  /** Name field error is shown only after a failed submit, until the user interacts with the field. */
  protected readonly showNameErrorAfterSubmit = signal(false);

  protected onNameInteraction(): void {
    this.showNameErrorAfterSubmit.set(false);
    this.organizations.clearCreateError();
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.showNameErrorAfterSubmit.set(true);
      return;
    }

    this.showNameErrorAfterSubmit.set(false);
    const { name } = this.form.getRawValue();
    this.organizations
      .createOrganization({ name: name.trim() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: () => {
        this.showNameErrorAfterSubmit.set(false);
        this.form.reset();
      },
      error: () => {
        /* message shown via organizations.createError */
      },
    });
  }

  protected get name() {
    return this.form.controls.name;
  }
}

