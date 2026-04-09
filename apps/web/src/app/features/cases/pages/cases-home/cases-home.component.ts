import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CaseFiltersComponent } from '../../components/case-filters/case-filters.component';
import { CaseItem, CasePriority, CaseStatus } from '../../models/cases.types';
import { CaseTableComponent } from '../../components/case-table/case-table.component';
import { CaseSummaryCardsComponent } from '../../components/case-summary-cards/case-summary-cards.component';
import { CasesService } from '@app/core/cases/cases.service';
import { CaseListQueryParams } from '@app/core/cases/cases-api.service';
import { priorityToApiCode, statusToApiCode } from '../../models/case-code-maps';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-cases-home',
  standalone: true,
  imports: [RouterLink, CaseFiltersComponent, CaseTableComponent, CaseSummaryCardsComponent],
  templateUrl: './cases-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CasesHomeComponent {
  constructor() {
    effect(() => {
      this.searchTerm();
      this.selectedStatus();
      this.selectedPriority();
      this.sortBy();

      this.loading.set(true);
      this.loadError.set(null);

      this.casesService
        .listCases(this.buildListQueryParams())
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (result) => {
            this.cases.set(result.items);
            this.loading.set(false);
          },
          error: () => {
            this.loadError.set('Could not load cases. Try again.');
            this.loading.set(false);
          },
        });
    });
  }
  private readonly casesService = inject(CasesService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly cases = signal<CaseItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly searchTerm = signal('');
  protected readonly selectedStatus = signal<'All' | CaseStatus>('All');
  protected readonly selectedPriority = signal<'All' | CasePriority>('All');
  protected readonly sortBy = signal<'updatedAt' | 'priority'>('updatedAt');

  private buildListQueryParams(): CaseListQueryParams {
    const search = this.searchTerm().trim();
    const status = this.selectedStatus();
    const priority = this.selectedPriority();
    const sort = this.sortBy();

    return {
      limit: 10,
      search: search.length > 0 ? search : undefined,
      status: status === 'All' ? undefined : statusToApiCode(status),
      priority: priority === 'All' ? undefined : priorityToApiCode(priority),
      sort: sort === 'updatedAt' ? 'UPDATED_AT' : 'PRIORITY',
      sortDescending: sort === 'priority',
    };
  }

  protected readonly visibleCases = computed(() => {
    const sort = this.sortBy();
    const list = [...this.cases()];

    if (sort === 'updatedAt') {
      list.sort((a, b) => a.updatedAt.localeCompare(b.updatedAt));
    } else {
      const order: Record<CasePriority, number> = { High: 0, Medium: 1, Low: 2 };
      list.sort((a, b) => order[a.priority] - order[b.priority]);
    }
    return list;
  });

  protected updateSearchTerm(value: string): void {
    this.searchTerm.set(value);
  }

  protected updateSelectedStatus(value: 'All' | CaseStatus): void {
    this.selectedStatus.set(value);
  }

  protected updateSelectedPriority(value: 'All' | CasePriority): void {
    this.selectedPriority.set(value);
  }

  protected updateSortBy(value: 'updatedAt' | 'priority'): void {
    this.sortBy.set(value);
  }

  protected readonly openCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'Open').length,
  );

  protected readonly totalCasesCount = computed(() => this.cases().length);

  protected readonly highPriorityCount = computed(
    () => this.cases().filter((caseItem) => caseItem.priority === 'High').length,
  );

  protected readonly inProgressCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'In Progress').length,
  );

  protected readonly closedCasesCount = computed(
    () => this.cases().filter((caseItem) => caseItem.status === 'Closed').length,
  );

  protected readonly summaryCards = computed(() => [
    {
      label: 'Total Cases',
      value: this.totalCasesCount(),
      helperText: 'All cases in the system',
    },
    {
      label: 'Open Cases',
      value: this.openCasesCount(),
      helperText: 'Require attention',
    },
    {
      label: 'In Progress',
      value: this.inProgressCasesCount(),
      helperText: 'Currently being worked',
    },
    {
      label: 'High Priority',
      value: this.highPriorityCount(),
      helperText: 'Need urgent review',
    },
    {
      label: 'Closed Cases',
      value: this.closedCasesCount(),
      helperText: 'Completed items',
    },
  ]);
}
