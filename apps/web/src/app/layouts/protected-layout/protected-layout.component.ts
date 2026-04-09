import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { debounceTime, filter, fromEvent } from 'rxjs';
import { ProtectedHeaderComponent } from './components/protected-header/protected-header.component';
import { ProtectedSidebarComponent } from './components/protected-sidebar/protected-sidebar.component';
import { ProtectedLayoutService } from './protected-layout.service';

const PROTECTED_LAYOUT_HTML_CLASS = 'protected-layout-active';

@Component({
  selector: 'app-protected-layout',
  standalone: true,
  imports: [RouterOutlet, ProtectedHeaderComponent, ProtectedSidebarComponent],
  templateUrl: './protected-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProtectedLayoutService],
  host: {
    class:
      'block h-dvh max-h-dvh overflow-hidden text-gray-200 bg-[radial-gradient(circle_at_top_left,rgba(59,130,246,0.16),transparent_28%),radial-gradient(circle_at_bottom_right,rgba(99,102,241,0.12),transparent_26%),linear-gradient(180deg,#0f172a_0%,#111827_100%)]',
  },
})
export class ProtectedLayoutComponent {
  protected readonly layout = inject(ProtectedLayoutService);
  private readonly router = inject(Router);

  constructor() {
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
