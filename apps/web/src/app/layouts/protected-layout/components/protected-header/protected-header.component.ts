import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { authUserDisplayName } from '@app/core/auth/parse-access-token-session';
import { AuthService } from '@app/core/auth/auth.service';
import { ProtectedLayoutService } from '../../protected-layout.service';
import { filter, fromEvent } from 'rxjs';

@Component({
  selector: 'app-protected-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './protected-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'contents',
  },
})
export class ProtectedHeaderComponent {
  protected readonly auth = inject(AuthService);
  protected readonly layout = inject(ProtectedLayoutService);
  private readonly router = inject(Router);

  protected readonly userMenuOpen = signal(false);

  protected readonly userDisplayName = computed(() => {
    const s = this.auth.session();
    return s ? authUserDisplayName(s) : '';
  });

  protected readonly userInitials = computed(() => {
    const s = this.auth.session();
    if (!s) {
      return '?';
    }
    const f = s.firstName?.trim() ?? '';
    const l = s.lastName?.trim() ?? '';
    if (f && l) {
      return (f[0]! + l[0]!).toUpperCase();
    }
    if (f) {
      return f.length >= 2 ? f.slice(0, 2).toUpperCase() : `${f[0]!}?`.toUpperCase();
    }
    if (l) {
      return l.length >= 2 ? l.slice(0, 2).toUpperCase() : `${l[0]!}?`.toUpperCase();
    }
    return '?';
  });

  constructor() {
    fromEvent(document, 'click')
      .pipe(
        filter(() => this.userMenuOpen()),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.userMenuOpen.set(false));

    fromEvent<KeyboardEvent>(document, 'keydown')
      .pipe(
        filter((e) => e.key === 'Escape' && this.userMenuOpen()),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.userMenuOpen.set(false));
  }

  protected toggleUserMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.userMenuOpen.update((open) => !open);
  }

  protected closeUserMenu(): void {
    this.userMenuOpen.set(false);
  }

  protected logout(): void {
    this.userMenuOpen.set(false);
    this.auth.signOut().subscribe({
      complete: () => void this.router.navigateByUrl('/auth/sign-in'),
    });
  }
}
