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
import { FirstResponseTimeDayPointDto } from '@app/core/cases/cases-api.service';
import { ChartContainerComponent } from '@app/shared/charts/chart-container/chart-container.component';
import { buildCartesianChartOptions } from '@app/shared/charts/chart-default-options';
import { chartSeriesStyle } from '@app/shared/charts/chart-palette';
import { BaseChartDirective } from 'ng2-charts';
import type { ChartData, ChartOptions } from 'chart.js';
import { catchError, finalize, of } from 'rxjs';

@Component({
  selector: 'app-dashboard-first-response-time-chart',
  imports: [ChartContainerComponent, BaseChartDirective],
  templateUrl: './dashboard-first-response-time-chart.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardFirstResponseTimeChartComponent {
  private readonly auth = inject(AuthService);
  private readonly cases = inject(CasesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly chartHeightPx = input(200);

  protected readonly windowDays = 30;

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly points = signal<FirstResponseTimeDayPointDto[] | null>(null);

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
    if (p === null || p.length === 0) {
      return true;
    }
    return p.every((row) => row.casesWithFirstResponse === 0);
  });

  protected readonly emptyMessage = computed(() => {
    if (!this.hasOrg()) {
      return 'Select an active organization in the header to see first response trends.';
    }
    return 'No qualifying first responses in this range.';
  });

  protected readonly lineChartData = computed((): ChartData<'line'> | null => {
    const rows = this.points();
    if (!rows?.length) {
      return null;
    }
    const labels = rows.map((r) => this.formatDayLabel(r.date));
    const style = chartSeriesStyle(0);
    return {
      labels,
      datasets: [
        {
          label: 'Avg. first response (minutes)',
          data: rows.map((r) =>
            r.averageFirstResponseMinutes != null ? r.averageFirstResponseMinutes : null,
          ),
          borderColor: style.border,
          backgroundColor: style.background,
          fill: false,
          tension: 0.25,
          borderWidth: 2,
          spanGaps: false,
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
        .getFirstResponseTimeOverTime(this.windowDays)
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          catchError(() => {
            if (!cancelled) {
              this.error.set('Could not load first response times. Try again.');
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
        tooltip: {
          callbacks: {
            label: (ctx: { parsed?: { y: number | null }; dataset?: { label?: string } }) => {
              const v = ctx.parsed?.y;
              if (v == null || Number.isNaN(v)) {
                return `${ctx.dataset?.label ?? ''}: —`;
              }
              return `${ctx.dataset?.label ?? ''}: ${formatMinutesLabel(v)}`;
            },
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            precision: 0,
            font: { size: 10 },
            callback: (value: string | number) => formatMinutesLabel(Number(value)),
          },
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

function formatMinutesLabel(minutes: number): string {
  if (!Number.isFinite(minutes)) {
    return '—';
  }
  if (minutes < 60) {
    return `${Math.round(minutes)}m`;
  }
  const h = Math.floor(minutes / 60);
  const m = Math.round(minutes % 60);
  return m === 0 ? `${h}h` : `${h}h ${m}m`;
}
