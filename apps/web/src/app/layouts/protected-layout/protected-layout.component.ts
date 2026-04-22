import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
import { NotificationsHubService } from '@app/core/realtime/notifications-hub.service';
import { debounceTime, distinctUntilChanged, filter, fromEvent } from 'rxjs';
import { ProtectedHeaderComponent } from './components/protected-header/protected-header.component';
import { ProtectedSidebarComponent } from './components/protected-sidebar/protected-sidebar.component';
import { ProtectedLayoutService } from './protected-layout.service';

const PROTECTED_LAYOUT_HTML_CLASS = 'protected-layout-active';

@Component({
  selector: 'app-protected-layout',
  imports: [RouterOutlet, ProtectedHeaderComponent, ProtectedSidebarComponent],
  templateUrl: './protected-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProtectedLayoutService],
  host: {
    class: 'protected-shell-bg block h-dvh max-h-dvh overflow-hidden text-gray-800',
  },
})
export class ProtectedLayoutComponent {
  protected readonly layout = inject(ProtectedLayoutService);
  private readonly router = inject(Router);

  constructor() {
    const auth = inject(AuthService);
    const notificationsHub = inject(NotificationsHubService);
    const destroyRef = inject(DestroyRef);

    toObservable(auth.accessTokenReadonly)
      .pipe(distinctUntilChanged(), takeUntilDestroyed(destroyRef))
      .subscribe((token) => {
        if (token) {
          void notificationsHub.ensureStarted();
        } else {
          void notificationsHub.stop();
        }
      });

    const doc = inject(DOCUMENT);
    inject(DestroyRef).onDestroy(() => {
      doc.documentElement.classList.remove(PROTECTED_LAYOUT_HTML_CLASS);
    });
    doc.documentElement.classList.add(PROTECTED_LAYOUT_HTML_CLASS);

    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.layout.closeMobileNav());

    fromEvent<KeyboardEvent>(document, 'keydown')
      .pipe(
        filter((e) => e.key === 'Escape' && this.layout.mobileNavOpen()),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.layout.closeMobileNav());

    fromEvent(window, 'resize')
      .pipe(debounceTime(150), takeUntilDestroyed())
      .subscribe(() => {
        if (window.matchMedia('(min-width: 981px)').matches) {
          this.layout.closeMobileNav();
        }
      });
  }
}

