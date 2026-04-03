import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EMPTY, finalize, tap } from 'rxjs';

@Component({
  selector: 'app-sign-in-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './sign-in-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignInFormComponent {
  private readonly fb = inject(FormBuilder);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false],
  });

  protected readonly isSubmitting = signal(false);

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const value = this.form.getRawValue();

    EMPTY.pipe(
      tap(() => {
        console.log('Sign in payload', value);
        // Replace EMPTY with this.auth.signIn(value) when the auth service exists.
      }),
      finalize(() => this.isSubmitting.set(false)),
    ).subscribe();
  }

  protected get email() {
    return this.form.controls.email;
  }

  protected get password() {
    return this.form.controls.password;
  }
}
