import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
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
  private readonly router = inject(Router);

  protected readonly userMenuOpen = signal(false);

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
