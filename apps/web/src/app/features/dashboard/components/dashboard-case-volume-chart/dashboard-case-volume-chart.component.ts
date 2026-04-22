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
import { CaseVolumeDayPointDto } from '@app/core/cases/cases-api.service';
import { ChartContainerComponent } from '@app/shared/charts/chart-container/chart-container.component';
import { buildCartesianChartOptions } from '@app/shared/charts/chart-default-options';
import { chartSeriesStyle } from '@app/shared/charts/chart-palette';
import { BaseChartDirective } from 'ng2-charts';
import type { ChartData, ChartOptions } from 'chart.js';
import { catchError, finalize, of } from 'rxjs';

@Component({
  selector: 'app-dashboard-case-volume-chart',
  imports: [ChartContainerComponent, BaseChartDirective],
  templateUrl: './dashboard-case-volume-chart.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardCaseVolumeChartComponent {
  private readonly auth = inject(AuthService);
  private readonly cases = inject(CasesService);
  private readonly destroyRef = inject(DestroyRef);

  /** Canvas height in CSS pixels — keep modest when shown in a multi-column dashboard row. */
  readonly chartHeightPx = input(200);

  /** Window length (API max 90). */
  protected readonly windowDays = 30;

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly points = signal<CaseVolumeDayPointDto[] | null>(null);

  protected readonly hasOrg = computed(() => {
    const id = this.auth.getEffectiveActiveOrganizationId();
    return id != null && id !== '';
  });

  protected readonly chartEmpty = computed(() => {
    if (this.loading()) {
      return false;
    }
    if (!this.hasOrg()) {
      return true;
    }
    const p = this.points();
    return p === null || p.length === 0;
  });

  protected readonly emptyMessage = computed(() => {
    if (!this.hasOrg()) {
      return 'Select an active organization in the header to see case trends.';
    }
    return 'No case activity in this range.';
  });

  protected readonly lineChartData = computed((): ChartData<'line'> | null => {
    const rows = this.points();
    if (!rows?.length) {
      return null;
    }
    const labels = rows.map((r) => this.formatDayLabel(r.date));
    const s0 = chartSeriesStyle(0);
    const s1 = chartSeriesStyle(1);
    const s2 = chartSeriesStyle(2);
    return {
      labels,
      datasets: [
        {
          label: 'Created',
          data: rows.map((r) => r.casesCreated),
          borderColor: s0.border,
          backgroundColor: s0.background,
          fill: false,
          tension: 0.25,
          borderWidth: 2,
        },
        {
          label: 'Resolved',
          data: rows.map((r) => r.casesResolved),
          borderColor: s1.border,
          backgroundColor: s1.background,
          fill: false,
          tension: 0.25,
          borderWidth: 2,
        },
        {
          label: 'Reopened',
          data: rows.map((r) => r.casesReopened),
          borderColor: s2.border,
          backgroundColor: s2.background,
          fill: false,
          tension: 0.25,
          borderWidth: 2,
        },
      ],
    };
  });

  protected readonly chartOptions = signal<ChartOptions<'line'>>(
    buildCartesianChartOptions<'line'>(this.compactLineChartOptionOverrides() as never),
  );

  constructor() {
    afterNextRender(() => {
      this.chartOptions.set(
        buildCartesianChartOptions<'line'>(
          this.compactLineChartOptionOverrides() as never,
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
        this.points.set(null);
        return;
      }

      if (!this.hasOrg()) {
        this.loading.set(false);
        this.error.set(null);
        this.points.set(null);
        return;
      }

      let cancelled = false;
      onCleanup(() => {
        cancelled = true;
      });

      this.error.set(null);
      this.loading.set(true);

      this.cases
        .getCaseVolumeOverTime(this.windowDays)
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          catchError(() => {
            if (!cancelled) {
              this.error.set('Could not load case volume. Try again.');
            }
            return of({ series: [] });
          }),
          finalize(() => {
            if (!cancelled) {
              this.loading.set(false);
            }
          }),
        )
        .subscribe((dto) => {
          if (!cancelled) {
            this.points.set(dto.series ?? []);
          }
        });
    });
  }

  protected formatDayLabel(isoDate: string): string {
    const [y, m, d] = isoDate.split('-').map((x) => parseInt(x, 10));
    if (!y || !m || !d) {
      return isoDate;
    }
    const dt = new Date(Date.UTC(y, m - 1, d));
    return dt.toLocaleDateString(undefined, { month: 'short', day: 'numeric', timeZone: 'UTC' });
  }

  /** Tighter typography and no axis titles so three charts can sit in one row on wide viewports. */
  private compactLineChartOptionOverrides() {
    return {
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            boxWidth: 10,
            padding: 8,
            font: { size: 10 },
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { precision: 0, font: { size: 10 } },
          title: { display: false },
        },
        x: {
          ticks: { maxRotation: 40, minRotation: 0, font: { size: 10 }, maxTicksLimit: 8 },
          title: { display: false },
        },
      },
    };
  }
}
