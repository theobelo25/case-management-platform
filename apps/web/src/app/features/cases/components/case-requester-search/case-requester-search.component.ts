import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { normalizeUserId } from '@app/core/auth/user-id-compare';
import { UserSearchHitDto } from '@app/core/users/users-api.service';

@Component({
  selector: 'app-case-requester-search',
  templateUrl: './case-requester-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseRequesterSearchComponent {
  /** Organization members (from org details) to search and pick from. */
  members = input<UserSearchHitDto[]>([]);
  loading = input(false);
  loadError = input<string | null>(null);
  disabled = input(false);
  /** Current selected member user id (empty = none). */
  selectedId = input<string>('');

  readonly selectionChange = output<string>();

  protected readonly searchDraft = signal('');

  protected readonly results = signal<UserSearchHitDto[]>([]);

  protected readonly selectedUser = computed(() => {
    const id = this.selectedId().trim();
    if (!id) return null;
    const want = normalizeUserId(id);
    return (
      this.members().find((m) => normalizeUserId(m.userId) === want) ?? null
    );
  });

  constructor() {
    effect(() => {
      const q = this.searchDraft().trim();
      const members = this.members();
      if (q.length === 0) {
        this.results.set([]);
        return;
      }
      const needle = q.toLowerCase();
      this.results.set(
        members.filter(
          (m) =>
            m.fullName.toLowerCase().includes(needle) ||
            m.email.toLowerCase().includes(needle),
        ),
      );
    });
  }

  protected updateSearch(value: string): void {
    this.searchDraft.set(value);
  }

  protected selectUser(user: UserSearchHitDto): void {
    this.selectionChange.emit(user.userId);
    this.searchDraft.set('');
    this.results.set([]);
  }

  protected clearSelection(): void {
    this.selectionChange.emit('');
  }

  protected isUserSelected(u: UserSearchHitDto): boolean {
    const want = normalizeUserId(this.selectedId());
    if (!want) {
      return false;
    }
    return normalizeUserId(u.userId) === want;
  }
}

