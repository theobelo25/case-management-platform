import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';

@Component({
  selector: 'app-protected-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './protected-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'contents',
  },
})
export class ProtectedHeaderComponent {
  protected readonly auth = inject(AuthService);
}
