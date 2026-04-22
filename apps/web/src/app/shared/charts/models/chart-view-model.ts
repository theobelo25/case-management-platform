import type { ChartConfiguration, ChartData, ChartOptions, ChartType } from 'chart.js';

/**
 * Presentation-only chart data — keep separate from HTTP DTOs; map in feature services or mappers.
 */
export type ChartAxisLabelsVm = readonly (string | number)[];

export interface ChartScalarSeriesVm {
  readonly label: string;
  readonly data: readonly number[];
}

/**
 * Line / bar charts that use a shared category axis.
 */
export interface CategoryChartDataVm<TType extends ChartType = ChartType> {
  readonly labels: ChartAxisLabelsVm;
  readonly datasets: ChartData<TType>['datasets'];
}

/**
 * Bundle passed to `canvas[baseChart]` — options are already merged with app defaults + overrides.
 */
export interface BaseChartPresentationVm<TType extends ChartType = ChartType> {
  readonly type: TType;
  readonly data: ChartConfiguration<TType>['data'];
  readonly options: ChartOptions<TType>;
}
