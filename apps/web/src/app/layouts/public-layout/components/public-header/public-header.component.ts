import { ViewportScroller } from '@angular/common';
import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-public-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './public-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicHeaderComponent {
  private readonly router = inject(Router);
  private readonly viewportScroller = inject(ViewportScroller);

  protected readonly mobileNavOpen = signal(false);

  protected toggleMobileNav(): void {
    const next = !this.mobileNavOpen();
    this.mobileNavOpen.set(next);
    if (next) {
      setTimeout(() => document.getElementById('public-header-mobile-nav-first')?.focus(), 0);
    }
  }

  protected closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  /**
   * When already on `/` with the same fragment, RouterLink would no-op; scroll manually.
   * Otherwise the router handles navigation + anchor scroll (see withInMemoryScrolling).
   */
  protected onInPageSectionClick(fragment: string, event: Event): void {
    this.closeMobileNav();

    const url = this.router.url;
    const [path, currentFrag] = url.split('#');
    const onHome = path === '' || path === '/';
    if (onHome && currentFrag === fragment) {
      event.preventDefault();
      this.viewportScroller.scrollToAnchor(fragment);
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.mobileNavOpen()) {
      this.closeMobileNav();
    }
  }
}
