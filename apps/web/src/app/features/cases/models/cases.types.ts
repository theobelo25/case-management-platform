export const CASE_STATUSES = ['Open', 'In Progress', 'Closed'] as const;
export type CaseStatus = (typeof CASE_STATUSES)[number];

export const CASE_PRIORITIES = ['Low', 'Medium', 'High'] as const;
export type CasePriority = (typeof CASE_PRIORITIES)[number];

export type CaseSortOption = 'updatedAt' | 'priority';

export interface CaseItem {
  id: string;
  title: string;
  description: string;
  status: CaseStatus;
  priority: CasePriority;
  updatedAt: string;
  assignee: string;
}
