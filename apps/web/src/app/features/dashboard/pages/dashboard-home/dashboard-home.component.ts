import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthResponseDto } from '@app/core/auth/auth-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { authUserDisplayName } from '@app/core/auth/parse-access-token-session';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  templateUrl: './dashboard-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHomeComponent {
  private readonly auth = inject(AuthService);

  protected sessionSnapshot(): AuthResponseDto | null {
    return this.auth.session();
  }

  protected userDisplayName(s: AuthResponseDto): string {
    return authUserDisplayName(s);
  }
}
