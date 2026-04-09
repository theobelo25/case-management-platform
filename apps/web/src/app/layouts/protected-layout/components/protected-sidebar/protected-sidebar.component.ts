import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ProtectedLayoutService } from '../../protected-layout.service';

@Component({
  selector: 'app-protected-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './protected-sidebar.component.html',
  styleUrl: './protected-sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'contents',
  },
})
export class ProtectedSidebarComponent {
  protected readonly layout = inject(ProtectedLayoutService);
}
