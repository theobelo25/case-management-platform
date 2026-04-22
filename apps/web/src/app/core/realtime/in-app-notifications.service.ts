import { computed, Injectable, signal } from '@angular/core';

export type CaseAssignmentAudience = 'new_assignee' | 'previous_assignee';

export interface CaseAssignmentNotificationPayload {
  type: 'case_assignment';
  audience: CaseAssignmentAudience;
  caseId: string;
  organizationId: string;
  caseTitle: string;
  message: string;
  createdAtUtc: string;
}

export type CaseEventSubtype = 'public_comment' | 'sla_breached' | 'sla_due_soon' | 'workload';

export interface CaseEventNotificationPayload {
  type: 'case_event';
  subtype: CaseEventSubtype;
  caseId: string;
  organizationId: string;
  caseTitle: string;
  message: string;
  createdAtUtc: string;
}

export type OrgMembershipAudience = 'added_member' | 'removed_member' | 'admin_audit_removal';

export interface OrgMembershipNotificationPayload {
  type: 'org_membership';
  audience: OrgMembershipAudience;
  organizationId: string;
  organizationName: string;
  message: string;
  createdAtUtc: string;
}

export type InAppNotificationPayload =
  | CaseAssignmentNotificationPayload
  | CaseEventNotificationPayload
  | OrgMembershipNotificationPayload;

export interface InAppNotificationItem {
  id: string;
  payload: InAppNotificationPayload;
  read: boolean;
  receivedAt: Date;
}

function asRecord(p: unknown): Record<string, unknown> | null {
  if (typeof p !== 'object' || p === null) {
    return null;
  }
  return p as Record<string, unknown>;
}

function typeName(o: Record<string, unknown>): string | undefined {
  const v = o['type'] ?? o['Type'];
  return typeof v === 'string' ? v : undefined;
}

/** Reads first matching string property (including empty string). */
function stringField(o: Record<string, unknown>, ...keys: string[]): string | undefined {
  for (const k of keys) {
    const v = o[k];
    if (typeof v === 'string') {
      return v;
    }
  }
  return undefined;
}

function nonEmptyStringField(o: Record<string, unknown>, ...keys: string[]): string | undefined {
  const s = stringField(o, ...keys);
  if (s === undefined) {
    return undefined;
  }
  const t = s.trim();
  return t.length > 0 ? t : undefined;
}

function parseCaseAssignmentPayload(p: unknown): CaseAssignmentNotificationPayload | null {
  const o = asRecord(p);
  if (!o || typeName(o) !== 'case_assignment') {
    return null;
  }
  const caseId = nonEmptyStringField(o, 'caseId', 'CaseId');
  const organizationId = nonEmptyStringField(o, 'organizationId', 'OrganizationId');
  const message = nonEmptyStringField(o, 'message', 'Message');
  const audience = stringField(o, 'audience', 'Audience');
  if (!caseId || !organizationId || !message) {
    return null;
  }
  if (audience !== 'new_assignee' && audience !== 'previous_assignee') {
    return null;
  }
  const caseTitle = stringField(o, 'caseTitle', 'CaseTitle') ?? '';
  const createdAtUtc =
    nonEmptyStringField(o, 'createdAtUtc', 'CreatedAtUtc') ?? new Date(0).toISOString();
  return {
    type: 'case_assignment',
    audience,
    caseId,
    organizationId,
    caseTitle,
    message,
    createdAtUtc,
  };
}

function parseCaseEventPayload(p: unknown): CaseEventNotificationPayload | null {
  const o = asRecord(p);
  if (!o || typeName(o) !== 'case_event') {
    return null;
  }
  const caseId = nonEmptyStringField(o, 'caseId', 'CaseId');
  const organizationId = nonEmptyStringField(o, 'organizationId', 'OrganizationId');
  const message = nonEmptyStringField(o, 'message', 'Message');
  const subtype = stringField(o, 'subtype', 'Subtype');
  if (!caseId || !organizationId || !message) {
    return null;
  }
  if (
    subtype !== 'public_comment' &&
    subtype !== 'sla_breached' &&
    subtype !== 'sla_due_soon' &&
    subtype !== 'workload'
  ) {
    return null;
  }
  const caseTitle = stringField(o, 'caseTitle', 'CaseTitle') ?? '';
  const createdAtUtc =
    nonEmptyStringField(o, 'createdAtUtc', 'CreatedAtUtc') ?? new Date(0).toISOString();
  return {
    type: 'case_event',
    subtype,
    caseId,
    organizationId,
    caseTitle,
    message,
    createdAtUtc,
  };
}

function parseOrgMembershipPayload(p: unknown): OrgMembershipNotificationPayload | null {
  const o = asRecord(p);
  if (!o || typeName(o) !== 'org_membership') {
    return null;
  }
  const organizationId = nonEmptyStringField(o, 'organizationId', 'OrganizationId');
  const message = nonEmptyStringField(o, 'message', 'Message');
  const audience = stringField(o, 'audience', 'Audience');
  if (!organizationId || !message) {
    return null;
  }
  if (
    audience !== 'added_member' &&
    audience !== 'removed_member' &&
    audience !== 'admin_audit_removal'
  ) {
    return null;
  }
  const organizationName = stringField(o, 'organizationName', 'OrganizationName') ?? '';
  const createdAtUtc =
    nonEmptyStringField(o, 'createdAtUtc', 'CreatedAtUtc') ?? new Date(0).toISOString();
  return {
    type: 'org_membership',
    audience,
    organizationId,
    organizationName,
    message,
    createdAtUtc,
  };
}

function parseHubPayload(payload: unknown): InAppNotificationPayload | null {
  return (
    parseCaseAssignmentPayload(payload) ??
    parseCaseEventPayload(payload) ??
    parseOrgMembershipPayload(payload)
  );
}

@Injectable({ providedIn: 'root' })
export class InAppNotificationsService {
  private static readonly maxItems = 50;

  readonly items = signal<InAppNotificationItem[]>([]);

  readonly unreadCount = computed(() => this.items().filter((i) => !i.read).length);

  ingestHubPayload(payload: unknown): void {
    const parsed = parseHubPayload(payload);
    if (!parsed) {
      return;
    }
    const item: InAppNotificationItem = {
      id: crypto.randomUUID(),
      payload: parsed,
      read: false,
      receivedAt: new Date(),
    };
    this.items.update((list) => [item, ...list].slice(0, InAppNotificationsService.maxItems));
  }

  markAllRead(): void {
    this.items.update((list) => list.map((i) => ({ ...i, read: true })));
  }

  dismiss(id: string): void {
    this.items.update((list) => list.filter((i) => i.id !== id));
  }

  clearAll(): void {
    this.items.set([]);
  }
}
