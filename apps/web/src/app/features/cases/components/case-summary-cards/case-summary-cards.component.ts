import { ChangeDetectionStrategy, Component, input } from '@angular/core';

interface SummaryCard {
  label: string;
  value: number;
  helperText: string;
}

@Component({
  selector: 'app-case-summary-cards',
  templateUrl: './case-summary-cards.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseSummaryCardsComponent {
  readonly cards = input.required<SummaryCard[]>();

  protected trackByLabel(_: number, card: SummaryCard): string {
    return card.label;
  }
}

