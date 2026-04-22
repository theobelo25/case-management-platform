import { CaseItem, CaseTimelineItem } from '@app/features/cases/models/cases.types';
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
  const name = row.assigneeName?.trim();
  if (name) return name;
  const uid = row.assigneeUserId?.trim();
  if (uid) {
    return 'Assigned user';
  }
  return 'Unassigned';
}

function resolveRequesterLabel(row: CaseResponseDto): string {
  const name = row.requesterName?.trim();
  if (name) return name;
  const id = row.requesterUserId?.trim();
  if (id) return `User ${id}`;
  return '—';
}

function resolveCreatorLabel(row: CaseResponseDto): string {
  const fromApi = row.createdByName?.trim();
  if (fromApi) return fromApi;
  const nested = row.createdBy?.displayName?.trim();
  if (nested) return nested;
  return row.createdByUserId;
}

function resolveTimeline(row: CaseResponseDto): CaseTimelineItem[] {
  if (!row.timeline) return [];

  return row.timeline
    .map((item) => {
      const timelineType: CaseTimelineItem['type'] = item.type === 'event' ? 'event' : 'message';
      const displayName = item.authorDisplayName?.trim();
      return {
        id: item.id,
        type: timelineType,
        createdAt: item.createdAtUtc,
        authorUserId: item.authorUserId,
        authorDisplayName: displayName ? displayName : null,
        body:
          timelineType === 'event'
            ? eventBodyFromMetadata(item.eventType, item.metadata)
            : item.body,
        isInternal: item.isInternal,
        isInitial: item.isInitial,
        eventType: item.eventType,
      };
    })
    .sort((a, b) => a.createdAt.localeCompare(b.createdAt));
}

function eventBodyFromMetadata(eventType: string | null, metadata: string | null): string {
  const parsed = safeJsonParse(metadata);
  const fromValue = parsed?.from;
  const toValue = parsed?.to;
  const fromUserId = parsed?.fromUserId;
  const toUserId = parsed?.toUserId;
  const fromName = parsed?.fromName?.trim();
  const toName = parsed?.toName?.trim();

  if (eventType === 'status_changed' && fromValue && toValue) {
    return `Status changed from ${fromValue} to ${toValue}.`;
  }

  if (eventType === 'priority_changed' && fromValue && toValue) {
    return `Priority changed from ${fromValue} to ${toValue}.`;
  }

  if (eventType === 'assignee_changed') {
    const fromLabel = fromName || fromUserId;
    const toLabel = toName || toUserId;
    if (toLabel && fromLabel) return `Assignee changed from ${fromLabel} to ${toLabel}.`;
    if (toLabel) return `Case assigned to ${toLabel}.`;
    if (fromLabel) return `${fromLabel} was removed as assignee.`;
    return 'Case was unassigned.';
  }

  if (eventType === 'case_archived') {
    return 'Case archived.';
  }

  if (eventType === 'case_unarchived') {
    return 'Case unarchived.';
  }

  if (eventType === 'sla_due_changed') {
    return 'SLA due changed.';
  }

  if (eventType === 'sla_breached') {
    return 'SLA breached.';
  }

  return eventType ? `${eventType.replace(/_/g, ' ')}.` : 'Case event recorded.';
}

function safeJsonParse(
  metadata: string | null,
): {
  from?: string;
  to?: string;
  fromUserId?: string;
  toUserId?: string;
  fromName?: string;
  toName?: string;
} | null {
  if (!metadata) return null;
  try {
    const parsed = JSON.parse(metadata) as {
      from?: unknown;
      to?: unknown;
      fromUserId?: unknown;
      toUserId?: unknown;
      fromName?: unknown;
      toName?: unknown;
    };
    return {
      from: typeof parsed.from === 'string' ? parsed.from : undefined,
      to: typeof parsed.to === 'string' ? parsed.to : undefined,
      fromUserId: typeof parsed.fromUserId === 'string' ? parsed.fromUserId : undefined,
      toUserId: typeof parsed.toUserId === 'string' ? parsed.toUserId : undefined,
      fromName: typeof parsed.fromName === 'string' ? parsed.fromName : undefined,
      toName: typeof parsed.toName === 'string' ? parsed.toName : undefined,
    };
  } catch {
    return null;
  }
}

function resolveOrgId(row: CaseResponseDto): string {
  const raw = row as CaseResponseDto & { OrganizationId?: string };
  return String(raw.organizationId ?? raw.OrganizationId ?? '');
}

function resolveIsArchived(row: CaseResponseDto): boolean {
  const raw = row as CaseResponseDto & { IsArchived?: boolean };
  if (typeof raw.isArchived === 'boolean') return raw.isArchived;
  if (typeof raw.IsArchived === 'boolean') return raw.IsArchived;
  return false;
}

function resolveSlaState(row: CaseResponseDto): string {
  const r = row as CaseResponseDto & { SlaState?: string };
  const v = row.slaState ?? r.SlaState;
  return typeof v === 'string' && v.trim() !== '' ? v.trim() : 'NONE';
}

function resolveOptionalIso(
  row: CaseResponseDto,
  camel: keyof CaseResponseDto,
  pascal: string,
): string | null | undefined {
  const r = row as CaseResponseDto & Record<string, unknown>;
  const a = r[camel];
  const b = r[pascal];
  if (typeof a === 'string') return a;
  if (typeof b === 'string') return b;
  if (a === null || b === null) return null;
  return undefined;
}

function resolveSlaRemainingSeconds(row: CaseResponseDto): number | null | undefined {
  const r = row as CaseResponseDto & { SlaRemainingSeconds?: number | null };
  const v = row.slaRemainingSeconds ?? r.SlaRemainingSeconds;
  if (v === null) return null;
  if (typeof v === 'number' && Number.isFinite(v)) return v;
  return undefined;
}

export function toCaseItem(row: CaseResponseDto): CaseItem {
  return {
    id: row.id,
    organizationId: resolveOrgId(row),
    title: row.title,
    description: resolveDescription(row),
    status: statusFromApiCode(row.status),
    priority: priorityFromApiCode(row.priority),
    slaState: resolveSlaState(row),
    slaDueAtUtc: resolveOptionalIso(row, 'slaDueAtUtc', 'SlaDueAtUtc'),
    slaBreachedAtUtc: resolveOptionalIso(row, 'slaBreachedAtUtc', 'SlaBreachedAtUtc'),
    slaPausedAtUtc: resolveOptionalIso(row, 'slaPausedAtUtc', 'SlaPausedAtUtc'),
    slaRemainingSeconds: resolveSlaRemainingSeconds(row),
    isArchived: resolveIsArchived(row),
    createdAt: row.createdAtUtc,
    updatedAt: row.updatedAtUtc,
    assigneeUserId: row.assigneeUserId ?? null,
    assignee: resolveAssignee(row),
    requesterLabel: resolveRequesterLabel(row),
    creatorLabel: resolveCreatorLabel(row),
    timeline: resolveTimeline(row),
  };
}

export function toCaseItems(rows: CaseResponseDto[]): CaseItem[] {
  return rows.map(toCaseItem);
}
