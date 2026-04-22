/**
 * Seconds remaining before due date at which we show "Due soon" instead of "On track".
 * Matches a typical "next business day" urgency window; override via options when calling {@link getSlaDerivedLabel}.
 */
export const SLA_DUE_SOON_THRESHOLD_SECONDS = 24 * 60 * 60;

/** Values returned by the API (`ResolveSlaState` on the case aggregate). */
export type SlaStateCode = 'NONE' | 'ACTIVE' | 'OVERDUE' | 'PAUSED' | 'BREACHED';

export interface SlaDerivedLabelInput {
  slaState?: string;
  slaDueAtUtc?: string | null;
  slaBreachedAtUtc?: string | null;
  slaPausedAtUtc?: string | null;
  slaRemainingSeconds?: number | null;
}

export interface GetSlaDerivedLabelOptions {
  /** Defaults to `new Date()` (caller's clock). Inject for tests. */
  now?: Date;
  /** Defaults to {@link SLA_DUE_SOON_THRESHOLD_SECONDS}. */
  dueSoonSeconds?: number;
}

function normalizeState(raw: string | undefined): SlaStateCode | string {
  const u = (raw ?? 'NONE').trim().toUpperCase();
  if (u === '') return 'NONE';
  return u;
}

export function formatDurationHuman(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds));
  if (s < 60) {
    return `${s} second${s === 1 ? '' : 's'}`;
  }
  const minutes = Math.floor(s / 60);
  if (minutes < 60) {
    return `${minutes} minute${minutes === 1 ? '' : 's'}`;
  }
  const hours = Math.floor(minutes / 60);
  if (hours < 48) {
    return `${hours} hour${hours === 1 ? '' : 's'}`;
  }
  const days = Math.floor(hours / 24);
  return `${days} day${days === 1 ? '' : 's'}`;
}

function remainingSecondsFromInput(input: SlaDerivedLabelInput, now: Date): number | null {
  const direct = input.slaRemainingSeconds;
  if (direct != null && Number.isFinite(direct)) {
    return direct;
  }
  const dueRaw = input.slaDueAtUtc;
  if (typeof dueRaw === 'string' && dueRaw.trim() !== '') {
    const dueMs = Date.parse(dueRaw);
    if (!Number.isNaN(dueMs)) {
      return Math.max(0, Math.floor((dueMs - now.getTime()) / 1000));
    }
  }
  return null;
}

function overdueSecondsFromInput(input: SlaDerivedLabelInput, now: Date): number {
  const dueRaw = input.slaDueAtUtc;
  const breachedRaw = input.slaBreachedAtUtc;

  // Once a breach is recorded, the SLA miss is fixed at (breach instant − due). Do not keep growing with wall clock.
  if (typeof breachedRaw === 'string' && breachedRaw.trim() !== '') {
    const breachMs = Date.parse(breachedRaw);
    if (!Number.isNaN(breachMs)) {
      if (typeof dueRaw === 'string' && dueRaw.trim() !== '') {
        const dueMs = Date.parse(dueRaw);
        if (!Number.isNaN(dueMs)) {
          return Math.max(0, Math.floor((breachMs - dueMs) / 1000));
        }
      }
      return 0;
    }
  }

  if (typeof dueRaw === 'string' && dueRaw.trim() !== '') {
    const dueMs = Date.parse(dueRaw);
    if (!Number.isNaN(dueMs)) {
      return Math.max(0, Math.floor((now.getTime() - dueMs) / 1000));
    }
  }
  const negRemain = input.slaRemainingSeconds;
  if (negRemain != null && Number.isFinite(negRemain) && negRemain < 0) {
    return Math.abs(negRemain);
  }
  return 0;
}

/**
 * User-facing SLA line derived from API SLA fields: "On track", "Due soon", "Overdue by …", etc.
 */
export function getSlaDerivedLabel(
  input: SlaDerivedLabelInput,
  options?: GetSlaDerivedLabelOptions,
): string {
  const now = options?.now ?? new Date();
  const dueSoon = options?.dueSoonSeconds ?? SLA_DUE_SOON_THRESHOLD_SECONDS;
  const state = normalizeState(input.slaState);

  if (state === 'NONE') {
    return 'No SLA';
  }
  if (state === 'PAUSED') {
    return 'Paused';
  }
  if (state === 'BREACHED' || state === 'OVERDUE') {
    const overdue = overdueSecondsFromInput(input, now);
    if (overdue <= 0) {
      return 'Overdue';
    }
    return `Overdue by ${formatDurationHuman(overdue)}`;
  }
  if (state === 'ACTIVE') {
    const remaining = remainingSecondsFromInput(input, now);
    if (remaining !== null && remaining <= dueSoon) {
      return 'Due soon';
    }
    return 'On track';
  }

  return state;
}

/** Row highlight when SLA is past due or breached (matches badge “danger” cases). */
export function isSlaRowOverdueHighlight(input: SlaDerivedLabelInput, options?: GetSlaDerivedLabelOptions): boolean {
  const now = options?.now ?? new Date();
  const state = normalizeState(input.slaState);
  if (state === 'BREACHED' || state === 'OVERDUE') {
    return true;
  }
  if (state === 'ACTIVE') {
    const remaining = remainingSecondsFromInput(input, now);
    if (remaining !== null && remaining <= 0) {
      return true;
    }
  }
  return false;
}

export type SlaBadgeTone = 'neutral' | 'success' | 'warning' | 'danger' | 'muted' | 'breach';

/** Tailwind classes for SLA status badges (table + detail). */
export const slaBadgeToneClasses: Record<SlaBadgeTone, string> = {
  neutral: 'bg-gray-100 text-gray-800 ring-1 ring-inset ring-gray-200/90',
  success: 'bg-emerald-100 text-emerald-900 ring-1 ring-inset ring-emerald-200/80',
  warning: 'bg-amber-100 text-amber-950 ring-1 ring-inset ring-amber-200/80',
  danger: 'bg-rose-100 text-rose-900 ring-1 ring-inset ring-rose-200/80',
  muted: 'bg-slate-100 text-slate-800 ring-1 ring-inset ring-slate-200/80',
  breach: 'bg-red-200 text-red-950 ring-1 ring-inset ring-red-300/90',
};

export function getSlaBadgeTone(input: SlaDerivedLabelInput, options?: GetSlaDerivedLabelOptions): SlaBadgeTone {
  const now = options?.now ?? new Date();
  const state = normalizeState(input.slaState);
  if (state === 'NONE') {
    return 'neutral';
  }
  if (state === 'PAUSED') {
    return 'muted';
  }
  if (state === 'BREACHED') {
    return 'breach';
  }
  if (state === 'OVERDUE') {
    return 'danger';
  }
  if (state === 'ACTIVE') {
    const remaining = remainingSecondsFromInput(input, now);
    if (remaining !== null && remaining <= 0) {
      return 'danger';
    }
    const dueSoon = options?.dueSoonSeconds ?? SLA_DUE_SOON_THRESHOLD_SECONDS;
    if (remaining !== null && remaining <= dueSoon) {
      return 'warning';
    }
    return 'success';
  }
  return 'neutral';
}

export interface SlaDetailParts {
  /** Short status line (On track, Due soon, …). */
  summary: string;
  /** Relative timing: “Due in …” / “Overdue by …”, or null when not applicable. */
  timingLine: string | null;
  isBreached: boolean;
  breachAtUtc: string | null | undefined;
}

/**
 * Copy for case detail: summary, optional relative timing, breach flag and timestamp for indicator.
 */
export function getSlaDetailParts(
  input: SlaDerivedLabelInput,
  options?: GetSlaDerivedLabelOptions,
): SlaDetailParts {
  const now = options?.now ?? new Date();
  const state = normalizeState(input.slaState);
  const summary = getSlaDerivedLabel(input, options);

  let timingLine: string | null = null;

  if (state === 'ACTIVE') {
    const remaining = remainingSecondsFromInput(input, now);
    if (remaining !== null) {
      if (remaining <= 0) {
        const overdue = overdueSecondsFromInput(input, now);
        timingLine = overdue <= 0 ? 'Past due' : `Overdue by ${formatDurationHuman(overdue)}`;
      } else {
        timingLine = `Due in ${formatDurationHuman(remaining)}`;
      }
    }
  }

  return {
    summary,
    timingLine,
    isBreached: state === 'BREACHED',
    breachAtUtc: input.slaBreachedAtUtc,
  };
}
