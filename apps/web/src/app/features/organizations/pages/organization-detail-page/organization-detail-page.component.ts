import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { OrganizationDetailSummaryComponent } from '../../components/organization-detail-summary/organization-detail-summary.component';
import { OrganizationMembersTableComponent } from '../../components/organization-members-table/organization-members-table.component';
import {
  OrganizationDetailViewModel,
  OrganizationMemberViewModel,
} from '../../models/organization-detail.view-model';
import { OrganizationsService } from '@app/core/organizations/organizations.service';
import {
  OrganizationDetailsResponseDto,
  UserMembershipResponseDto,
} from '@app/core/organizations/organizations-api.service';
import { HttpErrorResponse } from '@angular/common/http';

type OrganizationDetailVm =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: OrganizationDetailsResponseDto }
  | { status: 'error'; message: string };

@Component({
  selector: 'app-organization-detail-page',
  standalone: true,
  imports: [RouterLink, OrganizationDetailSummaryComponent, OrganizationMembersTableComponent],
  templateUrl: './organization-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly organizations = inject(OrganizationsService);

  /**
   * Placeholder data until the API exists. Uses the route id in the mock org name for clarity.
   */
  protected readonly organization = computed((): OrganizationDetailViewModel | null => {
    const state = this.detail();
    if (state.status !== 'success') return null;

    const org = state.data.organization;

    return {
      id: org.id,
      name: org.name,
      createdAtUtc: org.createdAtUtc,
    };
  });

  protected readonly members = computed((): OrganizationMemberViewModel[] => {
    const state = this.detail();
    if (state.status !== 'success') return [];
    return state.data.members.map(memberToViewModel);
  });

  protected readonly detail = toSignal(
    this.route.paramMap.pipe(
      map((p) => p.get('organizationId')),
      switchMap((id) => {
        if (id == null || id === '') return of<OrganizationDetailVm>({ status: 'idle' });

        return this.organizations.getOrganizationDetailsReadonly(id).pipe(
          map(
            (data): OrganizationDetailVm => ({
              status: 'success',
              data,
            }),
          ),
          startWith<OrganizationDetailVm>({ status: 'loading' }),
          catchError((err: unknown) =>
            of<OrganizationDetailVm>({
              status: 'error',
              message: messageFromHttp(err),
            }),
          ),
        );
      }),
    ),
    { initialValue: { status: 'idle' } satisfies OrganizationDetailVm },
  );
}

function messageFromHttp(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const detail = err.error?.detail;
    if (typeof detail === 'string') return detail;
    return `Request failed (${err.status}).`;
  }
  return 'Something went wrong.';
}

function memberToViewModel(m: UserMembershipResponseDto): OrganizationMemberViewModel {
  return {
    id: m.id,
    name: m.name,
    role: m.role,
  };
}
