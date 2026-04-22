import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CasePriority, CaseSortOption, CaseStatus } from '../../models/cases.types';

@Component({
  selector: 'app-case-filters',
  templateUrl: './case-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseFiltersComponent {
  readonly searchTerm = input.required<string>();
  readonly selectedStatus = input.required<'All' | CaseStatus>();
  readonly selectedPriority = input.required<'All' | CasePriority>();
  readonly sortBy = input.required<CaseSortOption>();
  readonly overdueOnly = input(false);

  protected readonly searchTermChange = output<string>();
  protected readonly selectedStatusChange = output<'All' | CaseStatus>();
  protected readonly selectedPriorityChange = output<'All' | CasePriority>();
  protected readonly sortByChange = output<CaseSortOption>();
  protected readonly overdueOnlyChange = output<boolean>();

  protected onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTermChange.emit(value);
  }

  protected onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as 'All' | CaseStatus;
    this.selectedStatusChange.emit(value);
  }

  protected onPriorityChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as 'All' | CasePriority;
    this.selectedPriorityChange.emit(value);
  }

  protected onSortChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as CaseSortOption;
    this.sortByChange.emit(value);
  }

  protected onOverdueOnlyChange(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.overdueOnlyChange.emit(checked);
  }
}

