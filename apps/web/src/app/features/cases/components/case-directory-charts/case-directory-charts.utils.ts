import { CaseItem } from '../../models/cases.types';

const HOUR_SEC = 3600;

/**
 * Seconds until SLA due (negative = overdue).
 * For non-paused cases, wall-clock delta from `slaDueAtUtc` is authoritative — the API often
 * sends `slaRemainingSeconds` clamped with `Max(0, …)`, which is 0 for overdue and mis-buckets
 * in charts. When paused, the clock is frozen so we prefer `slaRemainingSeconds` when set.
 */
export function getSecondsUntilSlaDue(c: CaseItem, nowMs: number = Date.now()): number | null {
  const fromDue = (): number | null => {
    const due = c.slaDueAtUtc;
    if (typeof due === 'string' && due.trim() !== '') {
      const t = Date.parse(due);
      if (!Number.isNaN(t)) {
        return Math.round((t - nowMs) / 1000);
      }
    }
    return null;
  };

  const paused = c.slaState?.trim().toUpperCase() === 'PAUSED';

  if (paused) {
    if (c.slaRemainingSeconds != null && Number.isFinite(c.slaRemainingSeconds)) {
      return c.slaRemainingSeconds;
    }
    return fromDue();
  }

  const dueSecs = fromDue();
  if (dueSecs !== null) {
    return dueSecs;
  }

  if (c.slaRemainingSeconds != null && Number.isFinite(c.slaRemainingSeconds)) {
    return c.slaRemainingSeconds;
  }
  return null;
}

export const SLA_BUCKET_LABELS = [
  'Overdue',
  '< 4h to due',
  '4–24h to due',
  '> 24h to due',
  'No SLA due',
] as const;

export type SlaBucketIndex = 0 | 1 | 2 | 3 | 4;

export function slaBucketIndexForCase(c: CaseItem, nowMs: number = Date.now()): SlaBucketIndex {
  const secs = getSecondsUntilSlaDue(c, nowMs);
  if (secs === null) {
    return 4;
  }
  if (secs < 0) {
    return 0;
  }
  if (secs < 4 * HOUR_SEC) {
    return 1;
  }
  if (secs < 24 * HOUR_SEC) {
    return 2;
  }
  return 3;
}

export function countSlaBuckets(cases: CaseItem[], nowMs: number = Date.now()): number[] {
  const counts = [0, 0, 0, 0, 0];
  for (const c of cases) {
    counts[slaBucketIndexForCase(c, nowMs)] += 1;
  }
  return counts;
}

const UNASSIGNED_KEY = '__unassigned__';

export interface AssigneeWorkloadSeries {
  labels: string[];
  counts: number[];
}

/**
 * Counts cases per assignee (by user id) for the given rows, sorted by count descending then label.
 * Unassigned cases share one bucket.
 */
export function buildAssigneeWorkloadSeries(cases: CaseItem[]): AssigneeWorkloadSeries {
  const countsByKey = new Map<string, number>();
  const labelByKey = new Map<string, string>();

  for (const c of cases) {
    const id = c.assigneeUserId?.trim();
    if (!id) {
      countsByKey.set(UNASSIGNED_KEY, (countsByKey.get(UNASSIGNED_KEY) ?? 0) + 1);
      labelByKey.set(UNASSIGNED_KEY, 'Unassigned');
      continue;
    }
    countsByKey.set(id, (countsByKey.get(id) ?? 0) + 1);
    if (!labelByKey.has(id)) {
      const name = c.assignee?.trim();
      labelByKey.set(id, name.length > 0 ? name : 'Assigned (unnamed)');
    }
  }

  const entries = [...countsByKey.entries()].map(([key, count]) => ({
    key,
    label: labelByKey.get(key) ?? key,
    count,
  }));

  entries.sort((a, b) => {
    if (b.count !== a.count) {
      return b.count - a.count;
    }
    return a.label.localeCompare(b.label, undefined, { sensitivity: 'base' });
  });

  return {
    labels: entries.map((e) => e.label),
    counts: entries.map((e) => e.count),
  };
}
