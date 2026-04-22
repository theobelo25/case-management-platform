import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@app/core/auth/auth.service';
import { CaseFormComponent } from '../../components/case-form/case-form.component';

@Component({
  selector: 'app-cases-new',
  imports: [RouterLink, CaseFormComponent],
  templateUrl: './cases-new.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CasesNewComponent implements OnInit {
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.auth.refreshUserProfile();
  }
}

