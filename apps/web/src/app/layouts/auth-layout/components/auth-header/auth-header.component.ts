import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-auth-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './auth-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthHeaderComponent {}
