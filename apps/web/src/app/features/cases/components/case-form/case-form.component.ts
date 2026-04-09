import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CasesService } from '@app/core/cases/cases.service';
import { CASE_PRIORITIES, CASE_STATUSES, CasePriority, CaseStatus } from '../../models/cases.types';
import { oneOfValidator } from '@app/shared/validators/oneOf.validator';
import { finalize } from 'rxjs';

type CaseFormMode = 'create' | 'update';

interface CaseFormInitialValue {
  title: string;
  description: string;
  priority: CasePriority;
  status: CaseStatus;
}

@Component({
  selector: 'app-case-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './case-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly casesService = inject(CasesService);
  private readonly router = inject(Router);

  @Input() public mode: CaseFormMode = 'create';
  @Input() public initialValue: CaseFormInitialValue | null = null;

  protected readonly statusOptions = CASE_STATUSES;
  protected readonly priorityOptions = CASE_PRIORITIES;
  public submitError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: ['', [Validators.required]],
    priority: this.fb.nonNullable.control<CasePriority>('Low', [
      Validators.required,
      oneOfValidator(CASE_PRIORITIES, 'invalidPriority'),
    ]),
    status: this.fb.nonNullable.control<CaseStatus>('Open', [
      Validators.required,
      oneOfValidator(CASE_STATUSES, 'invalidStatus'),
    ]),
  });

  protected readonly isSubmitting = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if ('initialValue' in changes && this.initialValue) {
      this.form.patchValue(this.initialValue, { emitEvent: false });
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const payload = this.form.getRawValue();
    const request$ =
      this.mode === 'update'
        ? this.casesService.updateCase(payload)
        : this.casesService.addCase(payload);

    request$.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: () => {
        const queryParams = this.mode === 'create' ? { created: '1' } : { updated: '1' };
        void this.router.navigate(['/app', 'cases'], { queryParams });
      },
      error: (err: unknown) => {
        let message = 'Something went wrong while saving this case.';

        if (err instanceof HttpErrorResponse) {
          if (err.status === 400) {
            message =
              typeof err.error?.detail === 'string'
                ? err.error.detail
                : 'Please check the form and try again.';
          } else if (err.status === 401) {
            message = 'You are not signed in or your session expired. Please sign in again.';
          } else if (err.status === 403) {
            message = 'You do not have permission to perform this action.';
          } else if (err.status === 422) {
            message =
              typeof err.error?.detail === 'string'
                ? err.error.detail
                : 'The server could not process this case.';
          } else if (err.status === 0) {
            message = 'Cannot reach the server. Check your connection.';
          }
        }

        this.submitError.set(message);
      },
    });
  }

  protected get title() {
    return this.form.controls.title;
  }

  protected get description() {
    return this.form.controls.description;
  }

  protected get priority() {
    return this.form.controls.priority;
  }

  protected get status() {
    return this.form.controls.status;
  }

  protected get isUpdateMode(): boolean {
    return this.mode === 'update';
  }
}
