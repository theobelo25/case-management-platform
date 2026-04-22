import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

let chartRegionSeq = 0;

@Component({
  selector: 'app-chart-container',
  templateUrl: './chart-container.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartContainerComponent {
  /** Fixed chart surface height in CSS pixels (parent width still controls horizontal size). */
  readonly heightPx = input(288);
  readonly loading = input(false);
  /** When true, shows the empty state instead of projected chart content. */
  readonly empty = input(false);
  readonly chartTitle = input<string | undefined>(undefined);
  /** Accessible name for the graphic when data is shown — describe trends or purpose. */
  readonly chartSummary = input('');
  readonly emptyMessage = input('No data to display');
  readonly loadingMessage = input('Loading chart');

  protected readonly regionId = `app-chart-region-${++chartRegionSeq}`;
  protected readonly titleId = computed(() =>
    this.chartTitle() ? `${this.regionId}-title` : null,
  );

  protected readonly regionAriaLabel = computed(() => {
    const t = this.chartTitle();
    if (t) {
      return null;
    }
    const s = this.chartSummary().trim();
    return s.length > 0 ? s : 'Chart';
  });

  protected readonly chartGraphicLabel = computed(() => {
    const s = this.chartSummary().trim();
    return s.length > 0 ? s : this.chartTitle()?.trim() ?? 'Chart data';
  });
}

