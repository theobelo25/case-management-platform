import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { OrganizationDetailViewModel } from '../../models/organization-detail-view-model';

@Component({
  selector: 'app-organization-detail-summary',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './organization-detail-summary.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationDetailSummaryComponent {
  readonly organization = input.required<OrganizationDetailViewModel>();
}
