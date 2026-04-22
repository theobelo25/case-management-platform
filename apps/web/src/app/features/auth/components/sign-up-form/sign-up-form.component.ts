import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '@app/core/auth/auth.service';
import { passwordsMatchValidator } from '@app/shared/validators/passwords-match.validator';

@Component({
  selector: 'app-sign-up-form',
  imports: [ReactiveFormsModule],
  templateUrl: './sign-up-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignUpFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  public submitError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]],
    },
    {
      validators: [passwordsMatchValidator()],
    },
  );

  protected readonly isSubmitting = signal(false);

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();

    this.auth
      .signUp({
        firstName: raw.firstName,
        lastName: raw.lastName,
        email: raw.email,
        password: raw.password,
        confirmPassword: raw.confirmPassword,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/app']);
        },
        error: (err: unknown) => {
          let message = 'Something went wrong. Please try again.';

          if (err instanceof HttpErrorResponse) {
            if (err.status === 409) {
              message = 'An account with this email already exists.';
            } else if (err.status === 400) {
              message = 'Check your details and try again.';
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

  protected get email() {
    return this.form.controls.email;
  }

  protected get password() {
    return this.form.controls.password;
  }

  protected get confirmPassword() {
    return this.form.controls.confirmPassword;
  }

  protected get acceptTerms() {
    return this.form.controls.acceptTerms;
  }

  protected get passwordsMismatch(): boolean {
    return this.form.hasError('passwordsMismatch') && this.confirmPassword.touched;
  }
}

