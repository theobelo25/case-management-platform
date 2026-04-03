import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-auth-footer',
  standalone: true,
  templateUrl: './auth-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthFooterComponent {}
