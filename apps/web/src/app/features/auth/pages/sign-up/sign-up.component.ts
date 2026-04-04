import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SignUpFormComponent } from '@app/features/auth/components/sign-up-form/sign-up-form.component';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [RouterLink, SignUpFormComponent],
  templateUrl: './sign-up.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignUpComponent {}
