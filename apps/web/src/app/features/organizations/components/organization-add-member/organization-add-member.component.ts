import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { UsersApiService, UserSearchHitDto } from '@app/core/users/users-api.service';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  EMPTY,
  finalize,
  of,
  switchMap,
  tap,
} from 'rxjs';

@Component({
  selector: 'app-organization-add-member',
  standalone: true,
  templateUrl: './organization-add-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationAddMemberComponent {
  private readonly usersApi = inject(UsersApiService);

  readonly existingMemberIds = input.required<string[]>();
  readonly isSubmitting = input(false);

  readonly cancelled = output<void>();
  readonly confirmed = output<string>();

  protected readonly searchDraft = signal('');
  protected readonly results = signal<UserSearchHitDto[]>([]);
  protected readonly nextCursor = signal<string | null>(null);
  protected readonly lastQuery = signal('');
  protected readonly searchLoading = signal(false);
  protected readonly searchError = signal<string | null>(null);
  protected readonly selectedUserId = signal<string | null>(null);
  protected readonly loadMoreLoading = signal(false);

  protected readonly visibleHits = computed(() => {
    const exclude = new Set(this.existingMemberIds());
    return this.results().filter((u) => !exclude.has(u.userId));
  });

  protected readonly emptyHint = computed(() => {
    if (this.searchLoading() || this.lastQuery().trim().length === 0) {
      return null;
    }
    if (this.visibleHits().length > 0) {
      return null;
    }
    if (this.results().length > 0) {
      return 'Everyone matching your search is already in this organization.';
    }
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
          if (q.length === 0) {
            this.results.set([]);
            this.nextCursor.set(null);
            this.lastQuery.set('');
            this.searchError.set(null);
            this.searchLoading.set(false);
            return EMPTY;
          }

          this.lastQuery.set(q);
          this.searchLoading.set(true);
          this.searchError.set(null);

          return this.usersApi.searchUsers(q).pipe(
            tap({
              next: (page) => {
                this.results.set(page.items);
                this.nextCursor.set(page.nextCursor ? page.nextCursor : null);
              },
            }),
            catchError((err: unknown) => {
              this.results.set([]);
              this.nextCursor.set(null);
              this.searchError.set(messageFromSearchHttp(err));
              return of(undefined);
            }),
            finalize(() => this.searchLoading.set(false)),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  protected loadMore(): void {
    const q = this.lastQuery();
    const cursor = this.nextCursor();
    if (!q || !cursor || this.loadMoreLoading()) {
      return;
    }

    this.loadMoreLoading.set(true);
    this.searchError.set(null);
    this.usersApi
      .searchUsers(q, cursor)
      .pipe(finalize(() => this.loadMoreLoading.set(false)))
      .subscribe({
        next: (page) => {
          const existingIds = new Set(this.results().map((u) => u.userId));
          const merged = [...this.results()];
          for (const item of page.items) {
            if (!existingIds.has(item.userId)) {
              existingIds.add(item.userId);
              merged.push(item);
            }
          }
          this.results.set(merged);
          this.nextCursor.set(page.nextCursor ? page.nextCursor : null);
        },
        error: (err: unknown) => this.searchError.set(messageFromSearchHttp(err)),
      });
  }

  protected selectUser(userId: string): void {
    this.selectedUserId.set(userId);
  }

  protected submit(): void {
    const id = this.selectedUserId();
    if (id != null) {
      this.confirmed.emit(id);
    }
  }
}

function messageFromSearchHttp(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const detail = err.error?.detail;
    if (typeof detail === 'string') {
      return detail;
    }
    return `Search failed (${err.status}).`;
  }
  return 'Search failed.';
}
