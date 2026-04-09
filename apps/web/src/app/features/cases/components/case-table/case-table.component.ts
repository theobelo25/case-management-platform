import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CaseItem, CasePriority, CaseStatus } from '../../models/cases.types';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-case-table',
  standalone: true,
  imports: [NgClass],
  templateUrl: './case-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaseTableComponent {
  readonly cases = input.required<CaseItem[]>();

  protected readonly priorityClassMap: Record<CasePriority, string> = {
    High: 'bg-rose-500/15 text-rose-200 ring-1 ring-inset ring-rose-400/30',
    Medium: 'bg-amber-500/15 text-amber-200 ring-1 ring-inset ring-amber-400/30',
    Low: 'bg-emerald-500/15 text-emerald-200 ring-1 ring-inset ring-emerald-400/30',
  };

  protected readonly statusClassMap: Record<CaseStatus, string> = {
    Open: 'bg-sky-500/15 text-sky-200 ring-1 ring-inset ring-sky-400/30',
    'In Progress': 'bg-violet-500/15 text-violet-200 ring-1 ring-inset ring-violet-400/30',
    Closed: 'bg-white/10 text-white/80 ring-1 ring-inset ring-white/15',
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
}
