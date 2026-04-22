import type { CasePriority, CaseStatus } from './cases.types';

export const CaseStatusCode = {
  New: 'new',
  Open: 'open',
  Pending: 'pending',
  Resolved: 'resolved',
  Closed: 'closed',
} as const;
export type CaseStatusCode = (typeof CaseStatusCode)[keyof typeof CaseStatusCode];

export const CasePriorityCode = {
  Low: 'low',
  Medium: 'medium',
  High: 'high',
} as const;
export type CasePriorityCode = (typeof CasePriorityCode)[keyof typeof CasePriorityCode];

const STATUS_TO_CODE: Record<CaseStatus, CaseStatusCode> = {
  New: CaseStatusCode.New,
  Open: CaseStatusCode.Open,
  Pending: CaseStatusCode.Pending,
  Resolved: CaseStatusCode.Resolved,
  Closed: CaseStatusCode.Closed,
};

const PRIORITY_TO_CODE: Record<CasePriority, CasePriorityCode> = {
  Low: CasePriorityCode.Low,
  Medium: CasePriorityCode.Medium,
  High: CasePriorityCode.High,
};

export function statusToApiCode(status: CaseStatus): CaseStatusCode {
  return STATUS_TO_CODE[status];
}

export function statusFromApiCode(code: string): CaseStatus {
  const normalized = code.trim().toUpperCase();

  switch (normalized) {
    case 'NEW':
      return 'New';
    case 'OPEN':
      return 'Open';
    case 'PENDING':
      return 'Pending';
    case 'RESOLVED':
      return 'Resolved';
    case 'CLOSED':
      return 'Closed';
    default:
      return 'Open';
  }
}

export function priorityToApiCode(priority: CasePriority): CasePriorityCode {
  return PRIORITY_TO_CODE[priority];
}

export function priorityFromApiCode(code: string): CasePriority {
  const normalized = code.trim().toUpperCase();

  switch (normalized) {
    case 'LOW':
      return 'Low';
    case 'MEDIUM':
      return 'Medium';
    case 'HIGH':
      return 'High';
    default:
      return 'Medium';
  }
}
