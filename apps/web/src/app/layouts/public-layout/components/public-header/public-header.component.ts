import { ViewportScroller } from '@angular/common';
import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-public-header',
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

  protected onInPageSectionClick(fragment: string, event: Event): void {
    event.preventDefault();
    this.closeMobileNav();

    const [path] = this.router.url.split('#');
    const onHome = path === '' || path === '/';
    if (onHome) {
      this.viewportScroller.scrollToAnchor(fragment);
      return;
    }

    void this.router.navigate(['/'], { fragment }).then((didNavigate) => {
      if (didNavigate) {
        setTimeout(() => this.viewportScroller.scrollToAnchor(fragment), 0);
      }
    });
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.mobileNavOpen()) {
      this.closeMobileNav();
    }
  }
}

