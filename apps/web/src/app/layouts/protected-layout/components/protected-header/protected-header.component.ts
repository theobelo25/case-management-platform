import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import type { MeResponseDto } from '@app/core/auth/auth-api.service';
import { authUserDisplayName } from '@app/core/auth/parse-access-token-session';
import { AuthService } from '@app/core/auth/auth.service';
import { ProtectedLayoutService } from '../../protected-layout.service';
import { filter, finalize, fromEvent } from 'rxjs';

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
  protected readonly orgSwitchPending = signal(false);
  protected readonly orgSwitchError = signal<string | null>(null);

  protected readonly userDisplayName = computed(() => {
    const s = this.auth.session();
    return s ? authUserDisplayName(s) : '';
  });

  /** Role and organization name for the active membership (from `GET /auth/me`). */
  protected readonly activeOrgSummary = computed(() => {
    const p = this.auth.userProfile();
    if (!p?.organizations?.length) {
      return null;
    }
    const active =
      this.findOrganization(p, p.activeOrganizationId) ?? p.organizations[0]!;
    return { role: active.role, name: active.name };
  });

  protected readonly organizationOptions = computed(() => this.auth.userProfile()?.organizations ?? []);

  /** Canonical id string from the list so `<select [value]>` matches an `<option [value]>`. */
  protected readonly selectedOrganizationId = computed(() => {
    const p = this.auth.userProfile();
    if (!p?.organizations?.length) {
      return '';
    }
    const match = this.findOrganization(p, p.activeOrganizationId);
    return match?.id ?? p.organizations[0]!.id;
  });

  /** `false` when only one membership — select is shown but switching is a no-op. */
  protected readonly canSwitchOrganization = computed(() => this.organizationOptions().length > 1);

  private findOrganization(
    profile: MeResponseDto,
    activeOrganizationId: string | undefined,
  ): { id: string; name: string; role: string } | undefined {
    const key = activeOrganizationId?.trim().toLowerCase();
    if (!key) {
      return undefined;
    }
    return profile.organizations.find((o) => o.id.trim().toLowerCase() === key);
  }

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

  protected onOrganizationChange(event: Event): void {
    const el = event.target as HTMLSelectElement;
    const nextId = el.value;
    const previousId = this.selectedOrganizationId();
    if (
      !nextId ||
      nextId.trim().toLowerCase() === previousId.trim().toLowerCase()
    ) {
      return;
    }

    this.orgSwitchError.set(null);
    this.orgSwitchPending.set(true);
    this.auth
      .switchActiveOrganization(nextId)
      .pipe(finalize(() => this.orgSwitchPending.set(false)))
      .subscribe({
        error: () => {
          this.orgSwitchError.set('Could not switch organization. Try again.');
          el.value = previousId;
        },
      });
  }
}
