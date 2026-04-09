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
const CODE_TO_STATUS: Record<CaseStatusCode, CaseStatus> = {
  OPEN: 'Open',
  IN_PROGRESS: 'In Progress',
  CLOSED: 'Closed',
};

const PRIORITY_TO_CODE: Record<CasePriority, CasePriorityCode> = {
  Low: CasePriorityCode.Low,
  Medium: CasePriorityCode.Medium,
  High: CasePriorityCode.High,
};
const CODE_TO_PRIORITY: Record<CasePriorityCode, CasePriority> = {
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
};

export function statusToApiCode(status: CaseStatus): CaseStatusCode {
  return STATUS_TO_CODE[status];
}

export function statusFromApiCode(code: string): CaseStatus {
  return CODE_TO_STATUS[code as CaseStatusCode];
}

export function priorityToApiCode(priority: CasePriority): CasePriorityCode {
  return PRIORITY_TO_CODE[priority];
}

export function priorityFromApiCode(code: string): CasePriority {
  return CODE_TO_PRIORITY[code as CasePriorityCode];
}
