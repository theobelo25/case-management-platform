export const CASE_STATUSES = ['New', 'Open', 'Pending', 'Resolved', 'Closed'] as const;
export type CaseStatus = (typeof CASE_STATUSES)[number];

export const CASE_PRIORITIES = ['Low', 'Medium', 'High'] as const;
export type CasePriority = (typeof CASE_PRIORITIES)[number];

export type CaseSortOption = 'updatedAt' | 'priority' | 'slaDue';

export interface CaseTimelineItem {
  id: string;
  type: 'message' | 'event';
  createdAt: string;
  authorUserId: string | null;
  /** Resolved on the API from user profile (FullName). */
  authorDisplayName: string | null;
  body: string | null;
  /** Message flags from API; null for event rows (`bool?`). */
  isInternal: boolean | null;
  isInitial: boolean | null;
  eventType: string | null;
}

/** SLA summary from the API; see `getSlaDerivedLabel` for display text. */
export interface CaseItem {
  id: string;
  organizationId: string;
  title: string;
  description: string;
  status: CaseStatus;
  priority: CasePriority;
  /** Canonical codes: `NONE`, `ACTIVE`, `OVERDUE`, `PAUSED`, `BREACHED`. */
  slaState: string;
  slaDueAtUtc?: string | null;
  slaBreachedAtUtc?: string | null;
  slaPausedAtUtc?: string | null;
  slaRemainingSeconds?: number | null;
  isArchived: boolean;
  /** When the case was created (ISO-8601). */
  createdAt: string;
  updatedAt: string;
  /** Present when the case has an assignee; use for actions like unassign. */
  assigneeUserId: string | null;
  assignee: string;
  /** Display text for the party who requested the case (name, or fallback). */
  requesterLabel: string;
  /** User who created the case in the system (display name when API provides nested user, else id). */
  creatorLabel: string;
  timeline: CaseTimelineItem[];
}
