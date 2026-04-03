import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-protected-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './protected-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'contents',
  },
})
export class ProtectedSidebarComponent {}
