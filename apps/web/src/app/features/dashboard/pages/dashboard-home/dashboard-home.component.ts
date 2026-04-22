import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AuthResponseDto, MeOrganizationDto } from '@app/core/auth/auth-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { authUserDisplayName } from '@app/core/auth/parse-access-token-session';
import { normalizeUserId } from '@app/core/auth/user-id-compare';
import { CasesService } from '@app/core/cases/cases.service';
import { OrganizationsApiService } from '@app/core/organizations/organizations-api.service';
import { CaseTableComponent } from '@app/features/cases/components/case-table/case-table.component';
import { DashboardCaseVolumeChartComponent } from '@app/features/dashboard/components/dashboard-case-volume-chart/dashboard-case-volume-chart.component';
import { DashboardCasesByStatusChartComponent } from '@app/features/dashboard/components/dashboard-cases-by-status-chart/dashboard-cases-by-status-chart.component';
import { DashboardFirstResponseTimeChartComponent } from '@app/features/dashboard/components/dashboard-first-response-time-chart/dashboard-first-response-time-chart.component';
import { statusToApiCode } from '@app/features/cases/models/case-code-maps';
import { CaseItem } from '@app/features/cases/models/cases.types';
import { catchError, finalize, forkJoin, map, of } from 'rxjs';

@Component({
  selector: 'app-dashboard-home',
  imports: [
    RouterLink,
    CaseTableComponent,
    DashboardCaseVolumeChartComponent,
    DashboardCasesByStatusChartComponent,
    DashboardFirstResponseTimeChartComponent,
  ],
  templateUrl: './dashboard-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHomeComponent {
  private readonly auth = inject(AuthService);
  private readonly casesService = inject(CasesService);
  private readonly organizationsApi = inject(OrganizationsApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly statsLoading = signal(false);
  protected readonly statsError = signal<string | null>(null);
  /** Count from first page (up to 100); null when no org or not loaded. */
  protected readonly openCasesStat = signal<number | null>(null);
  protected readonly openCasesHasMore = signal(false);
  protected readonly pendingCasesStat = signal<number | null>(null);
  protected readonly pendingCasesHasMore = signal(false);
  protected readonly teamMembersStat = signal<number | null>(null);

  protected readonly assignedCases = signal<CaseItem[]>([]);
  protected readonly assignedCasesLoading = signal(false);
  protected readonly assignedCasesError = signal<string | null>(null);

  protected readonly assignedCasesSubheading = computed(() => {
    const n = this.assignedCases().length;
    if (n === 0) {
      return 'No cases assigned to you right now';
    }
    return `Showing ${n} assigned case(s)`;
  });

  /** From `GET /auth/me`; updates when profile loads or org switches. */
  protected readonly meProfile = computed(() => this.auth.userProfile());

  protected readonly myOrganizations = computed((): MeOrganizationDto[] => {
    return this.meProfile()?.organizations ?? [];
  });

  protected isActiveOrganization(orgId: string): boolean {
    const active = this.meProfile()?.activeOrganizationId?.trim();
    if (!active) {
      return false;
    }
    return normalizeUserId(active) === normalizeUserId(orgId);
  }

  constructor() {
    effect((onCleanup) => {
      this.auth.userProfile();
      const session = this.auth.session();
      if (!session) {
        this.resetDashboardStats();
        this.assignedCases.set([]);
        this.assignedCasesError.set(null);
        this.assignedCasesLoading.set(false);
        return;
      }

      const orgId = this.auth.getEffectiveActiveOrganizationId();
      if (!orgId) {
        this.resetDashboardStats();
      } else {
        let cancelledStats = false;
        onCleanup(() => {
          cancelledStats = true;
        });

        this.statsError.set(null);
        this.statsLoading.set(true);

        forkJoin({
          open: this.casesService.listCases({
            limit: 100,
            status: statusToApiCode('Open'),
            sort: 'UPDATED_AT',
            sortDescending: true,
          }),
          pending: this.casesService.listCases({
            limit: 100,
            status: statusToApiCode('Pending'),
            sort: 'UPDATED_AT',
            sortDescending: true,
          }),
          members: this.organizationsApi.getOrganizationDetails(orgId).pipe(
            map((d) => d.members.length),
            catchError(() => of(null)),
          ),
        })
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(() => {
              if (!cancelledStats) {
                this.statsError.set('Could not load dashboard stats.');
              }
              return of({
                open: { items: [], nextCursor: null, previousCursor: null },
                pending: { items: [], nextCursor: null, previousCursor: null },
                members: null as number | null,
              });
            }),
            finalize(() => {
              if (!cancelledStats) {
                this.statsLoading.set(false);
              }
            }),
          )
          .subscribe((r) => {
            if (cancelledStats) {
              return;
            }
            const hasCursor = (c: string | null) => c != null && c !== '';
            this.openCasesStat.set(r.open.items.length);
            this.openCasesHasMore.set(hasCursor(r.open.nextCursor));
            this.pendingCasesStat.set(r.pending.items.length);
            this.pendingCasesHasMore.set(hasCursor(r.pending.nextCursor));
            this.teamMembersStat.set(r.members);
          });
      }

      let cancelledAssigned = false;
      onCleanup(() => {
        cancelledAssigned = true;
      });

      this.assignedCasesLoading.set(true);
      this.assignedCasesError.set(null);

      this.casesService
        .listCases({
          limit: 20,
          assignedToMe: true,
          sort: 'UPDATED_AT',
          sortDescending: true,
        })
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          catchError(() => {
            if (!cancelledAssigned) {
              this.assignedCasesError.set('Could not load assigned cases. Try again.');
            }
            return of({ items: [], nextCursor: null, previousCursor: null });
          }),
          finalize(() => {
            if (!cancelledAssigned) {
              this.assignedCasesLoading.set(false);
            }
          }),
        )
        .subscribe((result) => {
          if (!cancelledAssigned) {
            const me = normalizeUserId(this.auth.session()?.userId);
            const mine = result.items.filter((c) => {
              const aid = normalizeUserId(c.assigneeUserId);
              return aid !== '' && aid === me;
            });
            this.assignedCases.set(mine);
          }
        });
    });
  }

  private resetDashboardStats(): void {
    this.statsLoading.set(false);
    this.statsError.set(null);
    this.openCasesStat.set(null);
    this.openCasesHasMore.set(false);
    this.pendingCasesStat.set(null);
    this.pendingCasesHasMore.set(false);
    this.teamMembersStat.set(null);
  }

  protected formatStatValue(count: number | null, hasMore: boolean): string {
    if (count === null) {
      return '—';
    }
    if (hasMore) {
      return `${count}+`;
    }
    return String(count);
  }

  protected sessionSnapshot(): AuthResponseDto | null {
    return this.auth.session();
  }

  protected userDisplayName(s: AuthResponseDto): string {
    return authUserDisplayName(s);
  }
}

