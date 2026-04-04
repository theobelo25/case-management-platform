import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthService } from '@app/core/auth/auth.service';
import { isInternalAppPath } from '@app/core/navigation/is-internal-app-path';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

const REMEMBERED_LOGIN_EMAIL_KEY = 'case-mgmt.remembered-login-email';

@Component({
  selector: 'app-sign-in-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './sign-in-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignInFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  public submitError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false],
  });

  protected readonly isSubmitting = signal(false);

  ngOnInit(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }
    const saved = localStorage.getItem(REMEMBERED_LOGIN_EMAIL_KEY);
    if (saved) {
      this.form.patchValue({ email: saved });
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const { email, password, rememberMe } = this.form.getRawValue();

    this.auth
      .signIn({ email, password })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          if (typeof localStorage !== 'undefined') {
            if (rememberMe) {
              localStorage.setItem(REMEMBERED_LOGIN_EMAIL_KEY, email);
            } else {
              localStorage.removeItem(REMEMBERED_LOGIN_EMAIL_KEY);
            }
          }
          const raw = this.route.snapshot.queryParamMap.get('returnUrl');
          const target = raw && isInternalAppPath(raw) ? raw : '/app';
          void this.router.navigateByUrl(target);
        },
        error: (err: unknown) => {
          let message = 'Something went wrong. Please try again.';

          if (err instanceof HttpErrorResponse) {
            if (err.status === 401) {
              message = 'Invalid email or password.';
            } else if (err.status === 400) {
              message = 'Check your email and password and try again.';
            } else if (err.status === 0) {
              message = 'Cannot reach the server. Check your connection.';
            }
          }

          this.submitError.set(message);
        },
      });
  }

  protected get email() {
    return this.form.controls.email;
  }

  protected get password() {
    return this.form.controls.password;
  }
}
