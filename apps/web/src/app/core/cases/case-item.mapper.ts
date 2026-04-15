import { CaseItem } from '@app/features/cases/models/cases.types';
import { CaseResponseDto } from './cases-api.service';
import { priorityFromApiCode, statusFromApiCode } from '@app/features/cases/models/case-code-maps';

function resolveDescription(row: CaseResponseDto): string {
  if (typeof row.description === 'string' && row.description.trim() !== '') {
    return row.description;
  }

  const initialMessage = row.timeline?.find((item) => item.type === 'message' && item.isInitial);
  return initialMessage?.body ?? '';
}

function resolveAssignee(row: CaseResponseDto): string {
  return row.createdBy?.displayName ?? row.requesterName ?? 'Unassigned';
}

export function toCaseItem(row: CaseResponseDto): CaseItem {
  return {
    id: row.id,
    title: row.title,
    description: resolveDescription(row),
    status: statusFromApiCode(row.status),
    priority: priorityFromApiCode(row.priority),
    updatedAt: row.updatedAtUtc,
    assignee: resolveAssignee(row),
  };
}

export function toCaseItems(rows: CaseResponseDto[]): CaseItem[] {
  return rows.map(toCaseItem);
}
