import { Injectable, signal } from '@angular/core';

@Injectable()
export class ProtectedLayoutService {
  readonly mobileNavOpen = signal(false);

  toggleMobileNav(): void {
    this.mobileNavOpen.update((open) => !open);
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  openMobileNav(): void {
    this.mobileNavOpen.set(true);
  }
}
