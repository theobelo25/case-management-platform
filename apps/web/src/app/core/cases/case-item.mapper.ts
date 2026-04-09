import { CaseItem } from '@app/features/cases/models/cases.types';
import { CaseResponseDto } from './cases-api.service';
import { priorityFromApiCode, statusFromApiCode } from '@app/features/cases/models/case-code-maps';

export function toCaseItem(row: CaseResponseDto): CaseItem {
  return {
    id: row.id,
    title: row.title,
    description: row.description,
    status: statusFromApiCode(row.status),
    priority: priorityFromApiCode(row.priority),
    updatedAt: row.updatedAtUtc,
    assignee: row.createdBy.displayName,
  };
}

export function toCaseItems(rows: CaseResponseDto[]): CaseItem[] {
  return rows.map(toCaseItem);
}
