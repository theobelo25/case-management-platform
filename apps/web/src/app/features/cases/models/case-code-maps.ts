import type { CasePriority, CaseStatus } from './cases.types';

export const CaseStatusCode = {
  Open: 'OPEN',
  InProgress: 'IN_PROGRESS',
  Closed: 'CLOSED',
} as const;
export type CaseStatusCode = (typeof CaseStatusCode)[keyof typeof CaseStatusCode];

export const CasePriorityCode = {
  Low: 'LOW',
  Medium: 'MEDIUM',
  High: 'HIGH',
} as const;
export type CasePriorityCode = (typeof CasePriorityCode)[keyof typeof CasePriorityCode];

const STATUS_TO_CODE: Record<CaseStatus, CaseStatusCode> = {
  Open: CaseStatusCode.Open,
  'In Progress': CaseStatusCode.InProgress,
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
    case 'OPEN':
    case 'NEW':
      return 'Open';
    case 'IN_PROGRESS':
    case 'PENDING':
      return 'In Progress';
    case 'CLOSED':
    case 'RESOLVED':
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
