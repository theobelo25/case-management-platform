import type { ChartData } from 'chart.js';

import { chartSeriesStyle } from './chart-palette';
import type { ChartScalarSeriesVm } from './models/chart-view-model';

/** Map dumb scalar series view-models to Chart.js line datasets using the shared series palette. */
export function lineDatasetsFromScalarSeries(
  series: readonly ChartScalarSeriesVm[],
): ChartData<'line'>['datasets'] {
  return series.map((s, i) => {
    const style = chartSeriesStyle(i);
    return {
      label: s.label,
      data: [...s.data],
      borderColor: style.border,
      backgroundColor: style.background,
      fill: 'origin',
      tension: 0.25,
      borderWidth: 2,
    };
  });
}
