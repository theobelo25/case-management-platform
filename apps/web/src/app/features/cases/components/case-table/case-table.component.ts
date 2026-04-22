import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { CaseItem, CasePriority, CaseStatus } from '../../models/cases.types';
import {
  getSlaBadgeTone,
  getSlaDerivedLabel,
  isSlaRowOverdueHighlight,
  slaBadgeToneClasses,
  type SlaBadgeTone,
} from '../../models/sla-derived-label';
import { DatePipe, NgClass } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-case-table',
  imports: [DatePipe, NgClass],
  templateUrl: './case-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseTableComponent {
  private readonly router = inject(Router);
  readonly cases = input.required<CaseItem[]>();
  /** When false, hides the Assignee column (e.g. “my tasks” lists). */
  readonly showAssigneeColumn = input(true);
  /** When true, shows a selection column for bulk triage. */
  readonly selectionMode = input(false);
  /** Case ids currently selected (when `selectionMode` is true). */
  readonly selectedIds = input<string[]>([]);
  readonly selectionChange = output<string[]>();
  readonly heading = input('Cases');
  readonly subheadingOverride = input<string | undefined>(undefined);

  protected readonly subheading = computed(() => {
    const custom = this.subheadingOverride();
    if (custom != null) {
      return custom;
    }
    return `Showing ${this.cases().length} case(s)`;
  });

  protected readonly priorityClassMap: Record<CasePriority, string> = {
    High: 'bg-rose-100 text-rose-800 ring-1 ring-inset ring-rose-200/80',
    Medium: 'bg-amber-100 text-amber-900 ring-1 ring-inset ring-amber-200/80',
    Low: 'bg-emerald-100 text-emerald-900 ring-1 ring-inset ring-emerald-200/80',
  };

  protected readonly statusClassMap: Record<CaseStatus, string> = {
    New: 'bg-blue-100 text-blue-900 ring-1 ring-inset ring-blue-200/80',
    Open: 'bg-sky-100 text-sky-900 ring-1 ring-inset ring-sky-200/80',
    Pending: 'bg-violet-100 text-violet-900 ring-1 ring-inset ring-violet-200/80',
    Resolved: 'bg-emerald-100 text-emerald-900 ring-1 ring-inset ring-emerald-200/80',
    Closed: 'bg-gray-100 text-gray-800 ring-1 ring-inset ring-gray-200/80',
  };

  protected trackByCaseId(_: number, caseItem: CaseItem): string {
    return caseItem.id;
  }

  protected getPriorityClasses(priority: CasePriority): string {
    return this.priorityClassMap[priority];
  }

  protected getStatusClasses(status: CaseStatus): string {
    return this.statusClassMap[status];
  }

  protected navigateToCase(caseId: string): void {
    void this.router.navigate(['/app/cases', caseId]);
  }

  protected isSelected(caseId: string): boolean {
    return this.selectedIds().includes(caseId);
  }

  protected readonly allVisibleSelected = computed(() => {
    const rows = this.cases();
    if (rows.length === 0) {
      return false;
    }
    const sel = new Set(this.selectedIds());
    return rows.every((c) => sel.has(c.id));
  });

  protected toggleSelectAll(checked: boolean): void {
    if (checked) {
      this.selectionChange.emit(this.cases().map((c) => c.id));
    } else {
      this.selectionChange.emit([]);
    }
  }

  protected toggleRow(caseId: string, checked: boolean): void {
    const next = new Set(this.selectedIds());
    if (checked) {
      next.add(caseId);
    } else {
      next.delete(caseId);
    }
    this.selectionChange.emit([...next]);
  }

  protected onCheckboxClick(event: Event): void {
    event.stopPropagation();
  }

  protected readonly slaToneClassMap = slaBadgeToneClasses;

  protected slaLabel(caseItem: CaseItem): string {
    return getSlaDerivedLabel(caseItem);
  }

  protected slaTone(caseItem: CaseItem): SlaBadgeTone {
    return getSlaBadgeTone(caseItem);
  }

  protected slaToneClasses(caseItem: CaseItem): string {
    return this.slaToneClassMap[this.slaTone(caseItem)];
  }

  protected rowOverdue(caseItem: CaseItem): boolean {
    return isSlaRowOverdueHighlight(caseItem);
  }
}

