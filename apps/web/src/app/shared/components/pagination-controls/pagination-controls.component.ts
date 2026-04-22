import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination-controls',
  templateUrl: './pagination-controls.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationControlsComponent {
  readonly mode = input<'offset' | 'cursor'>('offset');
  readonly totalCount = input.required<number>();
  readonly skip = input.required<number>();
  readonly limit = input.required<number>();
  readonly pageSizeOptions = input<number[]>([10, 25, 50, 100]);
  readonly showPageSizeSelector = input(true);
  readonly pageNumber = input(1);
  readonly canGoPreviousCursor = input(false);
  readonly canGoNextCursor = input(false);
  readonly disabled = input(false);

  readonly skipChange = output<number>();
  readonly limitChange = output<number>();
  readonly previousClick = output<void>();
  readonly nextClick = output<void>();

  protected readonly rangeLabel = computed(() => {
    const total = this.totalCount();
    const s = this.skip();
    const lim = this.limit();

    if (total === 0) return 'No results';

    const from = s + 1;
    const to = Math.min(s + lim, total);

    return `${from}-${to} of ${total}`;
  });

  protected readonly canGoPrevious = computed(() => this.skip() > 0);

  protected readonly canGoNext = computed(() => {
    const s = this.skip();
    const lim = this.limit();
    const total = this.totalCount();

    return s + lim < total;
  });

  protected readonly canGoPreviousResolved = computed(() =>
    this.mode() === 'cursor' ? this.canGoPreviousCursor() : this.canGoPrevious(),
  );

  protected readonly canGoNextResolved = computed(() =>
    this.mode() === 'cursor' ? this.canGoNextCursor() : this.canGoNext(),
  );

  protected readonly statusLabel = computed(() => {
    if (this.mode() === 'cursor') {
      return `Page ${this.pageNumber()}`;
    }
    return this.rangeLabel();
  });

  protected goPrevious(): void {
    if (!this.canGoPreviousResolved() || this.disabled()) return;

    if (this.mode() === 'cursor') {
      this.previousClick.emit();
      return;
    }

    const lim = this.limit();

    this.skipChange.emit(Math.max(0, this.skip() - lim));
  }

  protected goNext(): void {
    if (!this.canGoNextResolved() || this.disabled()) return;

    if (this.mode() === 'cursor') {
      this.nextClick.emit();
      return;
    }

    this.skipChange.emit(this.skip() + this.limit());
  }

  protected onLimitSelect(event: Event): void {
    const el = event.target as HTMLSelectElement;
    const v = Number(el.value);
    if (Number.isFinite(v) && v > 0) this.limitChange.emit(v);
  }
}

