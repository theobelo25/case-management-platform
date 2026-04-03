import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthFooterComponent } from './components/auth-footer/auth-footer.component';
import { AuthHeaderComponent } from './components/auth-header/auth-header.component';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, AuthHeaderComponent, AuthFooterComponent],
  templateUrl: './auth-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class:
      'block min-h-dvh text-gray-200 [background:radial-gradient(circle_at_top_left,rgba(59,130,246,0.18),transparent_30%),radial-gradient(circle_at_bottom_right,rgba(99,102,241,0.16),transparent_28%),linear-gradient(180deg,#0f172a_0%,#111827_100%)]',
  },
})
export class AuthLayoutComponent {}
