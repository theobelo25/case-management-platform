import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthResponseDto } from '@app/core/auth/auth-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { authUserDisplayName } from '@app/core/auth/parse-access-token-session';
import { ProfileFormComponent } from '../../components/profile-form/profile-form.component';

@Component({
  selector: 'app-dashboard-settings',
  imports: [ProfileFormComponent],
  templateUrl: './dashboard-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardSettingsComponent {
  private readonly auth = inject(AuthService);

  protected sessionSnapshot(): AuthResponseDto | null {
    return this.auth.session();
  }

  protected userDisplayName(s: AuthResponseDto): string {
    return authUserDisplayName(s);
  }
}

