import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { UserSearchHitDto } from '@app/core/users/users-api.service';
import { EMPTY, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-case-assign-modal',
  templateUrl: './case-assign-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseAssignModalComponent {
  readonly organizationMembers = input.required<UserSearchHitDto[]>();

  readonly cancelled = output<void>();
  readonly confirmed = output<UserSearchHitDto>();

  protected readonly searchDraft = signal('');
  protected readonly results = signal<UserSearchHitDto[]>([]);
  protected readonly lastQuery = signal('');
  protected readonly searchLoading = signal(false);
  protected readonly searchError = signal<string | null>(null);
  protected readonly selectedUserId = signal<string | null>(null);

  protected readonly selectedUser = computed(() => {
    const selectedId = this.selectedUserId();
    if (selectedId == null) return null;
    return this.results().find((u) => u.userId === selectedId) ?? null;
  });

  protected readonly emptyHint = computed(() => {
    if (this.searchLoading() || this.lastQuery().trim().length === 0) return null;
    if (this.results().length > 0) return null;
    return 'No users matched your search.';
  });

  constructor() {
    toObservable(this.searchDraft)
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((raw) => {
          const q = raw.trim();
          this.selectedUserId.set(null);
          this.searchError.set(null);

          if (q.length === 0) {
            this.results.set([]);
            this.lastQuery.set('');
            this.searchLoading.set(false);
            return EMPTY;
          }

          this.lastQuery.set(q);
          this.searchLoading.set(true);
          const needle = q.toLowerCase();
          const members = this.organizationMembers();
          const filtered = members.filter(
            (m) =>
              m.fullName.toLowerCase().includes(needle) || m.email.toLowerCase().includes(needle),
          );
          this.results.set(filtered);
          this.searchLoading.set(false);
          return of(undefined);
        }),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  protected selectUser(userId: string): void {
    this.selectedUserId.set(userId);
  }

  protected submit(): void {
    const user = this.selectedUser();
    if (user != null) this.confirmed.emit(user);
  }
}

