import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  signal,
} from '@angular/core';
import { CaseItem, CASE_PRIORITIES, CASE_STATUSES } from '../../models/cases.types';
import { ChartContainerComponent } from '@app/shared/charts/chart-container/chart-container.component';
import { buildCartesianChartOptions, buildChartOptions } from '@app/shared/charts/chart-default-options';
import { chartSeriesStyle } from '@app/shared/charts/chart-palette';
import { BaseChartDirective } from 'ng2-charts';
import type { ChartData, ChartOptions } from 'chart.js';
import {
  SLA_BUCKET_LABELS,
  buildAssigneeWorkloadSeries,
  countSlaBuckets,
} from './case-directory-charts.utils';

@Component({
  selector: 'app-case-directory-charts',
  imports: [ChartContainerComponent, BaseChartDirective],
  templateUrl: './case-directory-charts.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseDirectoryChartsComponent {
  readonly cases = input.required<CaseItem[]>();
  readonly loading = input(false);

  protected readonly mixMode = signal<'status' | 'priority'>('status');

  protected readonly chartEmpty = computed(() => !this.loading() && this.cases().length === 0);

  protected readonly emptyMessage = computed(() =>
    this.loading() ? 'Loading cases…' : 'No cases match the current filters.',
  );

  protected readonly doughnutChartData = computed((): ChartData<'doughnut'> | null => {
    const rows = this.cases();
    if (rows.length === 0) {
      return null;
    }
    if (this.mixMode() === 'status') {
      const labels: string[] = [];
      const data: number[] = [];
      const colors: string[] = [];
      let colorIdx = 0;
      for (const s of CASE_STATUSES) {
        const n = rows.filter((r) => r.status === s).length;
        if (n > 0) {
          labels.push(s);
          data.push(n);
          colors.push(chartSeriesStyle(colorIdx).border);
          colorIdx += 1;
        }
      }
      if (data.length === 0) {
        return null;
      }
      return {
        labels,
        datasets: [
          {
            data,
            backgroundColor: colors,
            hoverOffset: 4,
            borderWidth: 2,
            borderColor: 'rgb(255 255 255)',
          },
        ],
      };
    }

    const labels: string[] = [];
    const data: number[] = [];
    const colors: string[] = [];
    let colorIdx = 0;
    for (const p of CASE_PRIORITIES) {
      const n = rows.filter((r) => r.priority === p).length;
      if (n > 0) {
        labels.push(p);
        data.push(n);
        colors.push(chartSeriesStyle(colorIdx).border);
        colorIdx += 1;
      }
    }
    if (data.length === 0) {
      return null;
    }
    return {
      labels,
      datasets: [
        {
          data,
          backgroundColor: colors,
          hoverOffset: 4,
          borderWidth: 2,
          borderColor: 'rgb(255 255 255)',
        },
      ],
    };
  });

  protected readonly slaBarChartData = computed((): ChartData<'bar'> | null => {
    const rows = this.cases();
    if (rows.length === 0) {
      return null;
    }
    const counts = countSlaBuckets(rows);
    const border = [
      'rgb(220 38 38)',
      'rgb(234 88 12)',
      'rgb(217 119 6)',
      'rgb(5 150 105)',
      'rgb(148 163 184)',
    ];
    const background = [
      'rgb(220 38 38 / 0.82)',
      'rgb(234 88 12 / 0.82)',
      'rgb(217 119 6 / 0.82)',
      'rgb(5 150 105 / 0.82)',
      'rgb(148 163 184 / 0.55)',
    ];
    return {
      labels: [...SLA_BUCKET_LABELS],
      datasets: [
        {
          label: 'Cases',
          data: counts,
          backgroundColor: background,
          borderColor: border,
          borderWidth: 1,
        },
      ],
    };
  });

  protected readonly assigneeWorkloadChartData = computed((): ChartData<'bar'> | null => {
    const rows = this.cases();
    if (rows.length === 0) {
      return null;
    }
    const { labels, counts } = buildAssigneeWorkloadSeries(rows);
    if (labels.length === 0) {
      return null;
    }
    const background: string[] = [];
    const border: string[] = [];
    for (let i = 0; i < labels.length; i += 1) {
      const s = chartSeriesStyle(i);
      border.push(s.border);
      background.push(s.border.replace(/\)$/, ' / 0.78)'));
    }
    return {
      labels,
      datasets: [
        {
          label: 'Cases',
          data: counts,
          backgroundColor: background,
          borderColor: border,
          borderWidth: 1,
        },
      ],
    };
  });

  protected readonly assigneeWorkloadChartHeightPx = computed(() => {
    const rows = this.cases();
    if (rows.length === 0) {
      return 200;
    }
    const n = buildAssigneeWorkloadSeries(rows).labels.length;
    return Math.min(520, Math.max(200, 48 + n * 36));
  });

  protected readonly doughnutOptions = signal<ChartOptions<'doughnut'>>(
    buildChartOptions<'doughnut'>(this.doughnutOverrides() as never),
  );

  protected readonly barOptions = signal<ChartOptions<'bar'>>(
    buildCartesianChartOptions<'bar'>(this.barOverrides() as never),
  );

  protected readonly assigneeWorkloadBarOptions = signal<ChartOptions<'bar'>>(
    buildCartesianChartOptions<'bar'>(this.assigneeWorkloadBarOverrides() as never),
  );

  constructor() {
    afterNextRender(() => {
      const root = document.documentElement;
      this.doughnutOptions.set(buildChartOptions<'doughnut'>(this.doughnutOverrides() as never, root));
      this.barOptions.set(buildCartesianChartOptions<'bar'>(this.barOverrides() as never, root));
      this.assigneeWorkloadBarOptions.set(
        buildCartesianChartOptions<'bar'>(this.assigneeWorkloadBarOverrides() as never, root),
      );
    });
  }

  protected setMixMode(mode: 'status' | 'priority'): void {
    this.mixMode.set(mode);
  }

  private doughnutOverrides(): Partial<ChartOptions<'doughnut'>> {
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

  private barOverrides(): Partial<ChartOptions<'bar'>> {
    return {
      plugins: {
        legend: { display: false },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { precision: 0, font: { size: 10 } },
          title: { display: false },
        },
        x: {
          ticks: { font: { size: 10 }, maxRotation: 45, minRotation: 0 },
          title: { display: false },
        },
      },
    };
  }

  private assigneeWorkloadBarOverrides(): Partial<ChartOptions<'bar'>> {
    return {
      indexAxis: 'y',
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            title: (items) => {
              const i = items[0]?.dataIndex;
              if (i === undefined) {
                return '';
              }
              const chart = items[0]?.chart;
              const labels = chart?.data?.labels;
              const raw = Array.isArray(labels) ? labels[i] : null;
              return typeof raw === 'string' ? raw : String(raw ?? '');
            },
          },
        },
      },
      scales: {
        x: {
          beginAtZero: true,
          ticks: { precision: 0, font: { size: 10 } },
          title: { display: false },
        },
        y: {
          ticks: {
            font: { size: 10 },
            autoSkip: false,
            callback(this, tickValue) {
              const label = this.getLabelForValue(tickValue as number);
              const s = String(label);
              const max = 28;
              return s.length > max ? `${s.slice(0, max - 1)}…` : s;
            },
          },
          title: { display: false },
        },
      },
    };
  }
}

