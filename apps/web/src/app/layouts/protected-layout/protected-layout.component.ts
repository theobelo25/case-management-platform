import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ProtectedHeaderComponent } from './components/protected-header/protected-header.component';
import { ProtectedSidebarComponent } from './components/protected-sidebar/protected-sidebar.component';

@Component({
  selector: 'app-protected-layout',
  standalone: true,
  imports: [RouterOutlet, ProtectedHeaderComponent, ProtectedSidebarComponent],
  templateUrl: './protected-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class:
      'block min-h-dvh text-gray-200 bg-[radial-gradient(circle_at_top_left,rgba(59,130,246,0.16),transparent_28%),radial-gradient(circle_at_bottom_right,rgba(99,102,241,0.12),transparent_26%),linear-gradient(180deg,#0f172a_0%,#111827_100%)]',
  },
})
export class ProtectedLayoutComponent {}
