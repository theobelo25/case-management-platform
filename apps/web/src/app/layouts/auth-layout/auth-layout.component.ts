import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthFooterComponent } from './components/auth-footer/auth-footer.component';
import { AuthHeaderComponent } from './components/auth-header/auth-header.component';

const AUTH_LAYOUT_HTML_CLASS = 'auth-layout-active';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, AuthHeaderComponent, AuthFooterComponent],
  templateUrl: './auth-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'protected-shell-bg block min-h-dvh text-gray-800',
  },
})
export class AuthLayoutComponent {
  constructor() {
    const doc = inject(DOCUMENT);
    inject(DestroyRef).onDestroy(() => {
      doc.documentElement.classList.remove(AUTH_LAYOUT_HTML_CLASS);
    });
    doc.documentElement.classList.add(AUTH_LAYOUT_HTML_CLASS);
  }
}

