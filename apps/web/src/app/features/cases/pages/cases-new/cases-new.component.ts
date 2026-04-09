import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CaseFormComponent } from '../../components/case-form/case-form.component';

@Component({
  selector: 'app-cases-new',
  standalone: true,
  imports: [RouterLink, CaseFormComponent],
  templateUrl: './cases-new.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CasesNewComponent {}
