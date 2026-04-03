import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EMPTY, finalize, tap } from 'rxjs';

import { passwordsMatchValidator } from '../../../../shared/validators/passwords-match.validator';

@Component({
  selector: 'app-sign-up-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './sign-up-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignUpFormComponent {
  private readonly fb = inject(FormBuilder);

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

    const raw = this.form.getRawValue();

    EMPTY.pipe(
      tap(() => {
        console.log('Sign up payload', {
          firstName: raw.firstName,
          lastName: raw.lastName,
          email: raw.email,
          password: raw.password,
        });
        // Replace EMPTY with this.auth.signUp(payload) when the auth service exists.
      }),
      finalize(() => this.isSubmitting.set(false)),
    ).subscribe();
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
