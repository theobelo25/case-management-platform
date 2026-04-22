import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@app/core/auth/auth.service';
import { CasesService } from '@app/core/cases/cases.service';
import { CaseStatusCountsDto } from '@app/core/cases/cases-api.service';
import { ChartContainerComponent } from '@app/shared/charts/chart-container/chart-container.component';
import { buildChartOptions } from '@app/shared/charts/chart-default-options';
import { chartSeriesStyle } from '@app/shared/charts/chart-palette';
import { BaseChartDirective } from 'ng2-charts';
import type { ChartData, ChartOptions } from 'chart.js';
import { catchError, EMPTY, finalize } from 'rxjs';

const STATUS_LABELS = ['New', 'Open', 'Pending', 'Resolved', 'Closed'] as const;

@Component({
  selector: 'app-dashboard-cases-by-status-chart',
  imports: [ChartContainerComponent, BaseChartDirective],
  templateUrl: './dashboard-cases-by-status-chart.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardCasesByStatusChartComponent {
  private readonly auth = inject(AuthService);
  private readonly cases = inject(CasesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly chartHeightPx = input(200);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly counts = signal<CaseStatusCountsDto | null>(null);

  protected readonly hasOrg = computed(() => {
    const id = this.auth.getEffectiveActiveOrganizationId();
    return id != null && id !== '';
  });

  protected readonly totalCases = computed(() => {
    const c = this.counts();
    if (!c) {
      return 0;
    }
    return (
      c.newCount +
      c.openCount +
      c.pendingCount +
      c.resolvedCount +
      c.closedCount
    );
  });

  protected readonly chartEmpty = computed(() => {
    if (this.loading()) {
      return false;
    }
    if (!this.hasOrg()) {
      return true;
    }
    const c = this.counts();
    if (c === null) {
      return true;
    }
    return this.totalCases() === 0;
  });

  protected readonly emptyMessage = computed(() => {
    if (!this.hasOrg()) {
      return 'Select an active organization in the header to see status breakdown.';
    }
    return 'No cases yet in this organization.';
  });

  protected readonly doughnutChartData = computed((): ChartData<'doughnut'> | null => {
    const c = this.counts();
    if (!c || this.totalCases() === 0) {
      return null;
    }
    const data = [
      c.newCount,
      c.openCount,
      c.pendingCount,
      c.resolvedCount,
      c.closedCount,
    ];
    const backgrounds = STATUS_LABELS.map((_, i) => chartSeriesStyle(i).border);
    return {
      labels: [...STATUS_LABELS],
      datasets: [
        {
          data,
          backgroundColor: backgrounds,
          hoverOffset: 4,
          borderWidth: 2,
          borderColor: 'rgb(255 255 255)',
        },
      ],
    };
  });

  protected readonly chartOptions = signal<ChartOptions<'doughnut'>>(
    buildChartOptions<'doughnut'>(this.doughnutOptionOverrides() as never),
  );

  constructor() {
    afterNextRender(() => {
      this.chartOptions.set(
        buildChartOptions<'doughnut'>(
          this.doughnutOptionOverrides() as never,
          document.documentElement,
        ),
      );
    });

    effect((onCleanup) => {
      this.auth.session();
      this.auth.userProfile();

      if (!this.auth.session()) {
        this.loading.set(false);
        this.error.set(null);
        this.counts.set(null);
        return;
      }

      if (!this.hasOrg()) {
        this.loading.set(false);
        this.error.set(null);
        this.counts.set(null);
        return;
      }

      let cancelled = false;
      onCleanup(() => {
        cancelled = true;
      });

      this.error.set(null);
      this.counts.set(null);
      this.loading.set(true);

      this.cases
        .getCaseStatusCounts()
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          catchError(() => {
            if (!cancelled) {
              this.error.set('Could not load case status counts. Try again.');
            }
            return EMPTY;
          }),
          finalize(() => {
            if (!cancelled) {
              this.loading.set(false);
            }
          }),
        )
        .subscribe((dto) => {
          if (!cancelled) {
            this.counts.set(dto);
          }
        });
    });
  }

  private doughnutOptionOverrides() {
    return {
      cutout: '58%',
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            boxWidth: 10,
            padding: 6,
            font: { size: 10 },
          },
        },
      },
    };
  }
}
