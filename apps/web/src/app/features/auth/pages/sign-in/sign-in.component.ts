import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SignInFormComponent } from '@app/features/auth/components/sign-in-form/sign-in-form.component';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [RouterLink, SignInFormComponent],
  templateUrl: './sign-in.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignInComponent {}
