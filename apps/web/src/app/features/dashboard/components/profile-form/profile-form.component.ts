import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '@app/core/auth/auth.service';
import {
  optionalMinLength,
  passwordsMatchValidator,
  updateProfilePasswordGroupValidator,
} from '@app/shared/validators/passwords-match.validator';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-update-profile-form',
  imports: [ReactiveFormsModule],
  templateUrl: './profile-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly auth = inject(AuthService);

  /** Avoids repatching when `session()` is refreshed with the same name fields. */
  private lastSyncedSessionNameKey: string | null = null;

  public submitError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      currentPassword: [''],
      newPassword: ['', [optionalMinLength(8)]],
      confirmNewPassword: [''],
    },
    {
      validators: [
        passwordsMatchValidator({
          passwordKey: 'newPassword',
          confirmKey: 'confirmNewPassword',
        }),
        updateProfilePasswordGroupValidator(),
      ],
    },
  );

  constructor() {
    effect(() => {
      const session = this.auth.session();
      if (!session) {
        this.lastSyncedSessionNameKey = null;
        return;
      }

      const key = `${session.firstName?.trim() ?? ''}|${session.lastName?.trim() ?? ''}`;
      if (!key || this.lastSyncedSessionNameKey === key) {
        return;
      }

      this.lastSyncedSessionNameKey = key;
      this.form.patchValue(
        {
          firstName: session.firstName?.trim() ?? '',
          lastName: session.lastName?.trim() ?? '',
        },
        { emitEvent: false },
      );
    });
  }

  protected readonly isSubmitting = signal(false);

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitError.set(null);
    this.isSubmitting.set(true);

    const { firstName, lastName, currentPassword, newPassword, confirmNewPassword } =
      this.form.getRawValue();

    this.auth
      .updateProfile({
        firstName,
        lastName,
        currentPassword,
        newPassword,
        confirmNewPassword,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: () => {
          /* session updated via tap on AuthService */
        },
        error: (err: unknown) => {
          let message = 'Something went wrong. Please try again.';

          if (err instanceof HttpErrorResponse) {
            if (err.status === 401) {
              message = 'Current password is incorrect.';
            } else if (err.status === 400) {
              message =
                typeof err.error?.detail === 'string'
                  ? err.error.detail
                  : 'Check the form and try again.';
            } else if (err.status === 0) {
              message = 'Cannot reach the server. Check your connection.';
            }
          }

          this.submitError.set(message);
        },
      });
  }

  protected get firstName() {
    return this.form.controls.firstName;
  }

  protected get lastName() {
    return this.form.controls.lastName;
  }

  protected get currentPassword() {
    return this.form.controls.currentPassword;
  }

  protected get newPassword() {
    return this.form.controls.newPassword;
  }

  protected get confirmNewPassword() {
    return this.form.controls.confirmNewPassword;
  }

  protected get passwordsMismatch(): boolean {
    return (
      this.form.hasError('passwordsMismatch') &&
      (this.confirmNewPassword.touched || this.newPassword.touched)
    );
  }

  protected get currentPasswordRequiredForChange(): boolean {
    return this.form.hasError('currentPasswordRequired') && this.currentPassword.touched;
  }
}

