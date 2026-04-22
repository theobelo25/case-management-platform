import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  computed,
  DestroyRef,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { CasesService } from '@app/core/cases/cases.service';
import { AuthService } from '@app/core/auth/auth.service';
import type { MeResponseDto } from '@app/core/auth/auth-api.service';
import { OrganizationsApiService, UserMembershipResponseDto } from '@app/core/organizations/organizations-api.service';
import { CASE_PRIORITIES, CASE_STATUSES, CasePriority, CaseStatus } from '../../models/cases.types';
import { CaseRequesterSearchComponent } from '../case-requester-search/case-requester-search.component';
import { oneOfValidator } from '@app/shared/validators/one-of.validator';
import {
  catchError,
  distinctUntilChanged,
  filter,
  finalize,
  map,
  of,
  switchMap,
} from 'rxjs';

type CaseFormMode = 'create' | 'update';

interface CaseFormInitialValue {
  title: string;
  description: string;
  priority: CasePriority;
  status: CaseStatus;
}

@Component({
  selector: 'app-case-form',
  imports: [ReactiveFormsModule, CaseRequesterSearchComponent],
  templateUrl: './case-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseFormComponent implements OnChanges, OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly casesService = inject(CasesService);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly organizationsApi = inject(OrganizationsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly injector = inject(Injector);

  @Input() public mode: CaseFormMode = 'create';
  @Input() public initialValue: CaseFormInitialValue | null = null;

  protected readonly statusOptions = CASE_STATUSES;
  protected readonly priorityOptions = CASE_PRIORITIES;
  public submitError = signal<string | null>(null);
  protected readonly requesterMembers = signal<UserMembershipResponseDto[]>([]);
  protected readonly requesterLoading = signal(false);
  protected readonly requesterLoadError = signal<string | null>(null);

  /** Active organization members available as requester (same list as org details). */
  protected readonly requesterSearchHits = computed(() =>
    this.requesterMembers().map((m) => ({
      userId: m.id,
      fullName: m.name,
      email: m.email,
    })),
  );

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: ['', [Validators.required]],
    requesterUserId: [''],
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

  ngOnInit(): void {
    if (this.mode !== 'create') {
      return;
    }

    this.auth.refreshUserProfile();

    // `toObservable` must receive an injector when not called from an injection context (e.g. not
    // in a constructor/field initializer). Otherwise the underlying effect may not run and members
    // never load.
    toObservable(this.auth.userProfile, { injector: this.injector })
      .pipe(
        filter((p): p is MeResponseDto => p != null),
        map(() => this.auth.getEffectiveActiveOrganizationId()),
        distinctUntilChanged(),
        switchMap((orgId) => {
          if (!orgId) {
            this.requesterLoadError.set(
              'You need to belong to an organization before you can assign a requester.',
            );
            this.requesterMembers.set([]);
            return of(null);
          }

          this.requesterLoadError.set(null);
          this.requesterLoading.set(true);
          return this.organizationsApi.getOrganizationDetails(orgId).pipe(
            finalize(() => this.requesterLoading.set(false)),
            catchError(() => {
              this.requesterLoadError.set(
                'Could not load organization members for requester selection.',
              );
              return of(null);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((detail) => {
        if (!detail) {
          return;
        }
        const members = detail.members.filter((m) => m.isArchived !== true);
        this.requesterMembers.set(members);
      });
  }

  protected onRequesterUserIdChange(userId: string): void {
    this.form.controls.requesterUserId.setValue(userId);
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();
    const request$ =
      this.mode === 'update'
        ? this.casesService.updateCase(raw)
        : this.casesService.addCase({
            ...raw,
            requesterUserId: raw.requesterUserId.trim() !== '' ? raw.requesterUserId.trim() : null,
          });

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
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

