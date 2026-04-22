import type { ChartOptions, ChartType } from 'chart.js';

import type { ResolvedChartTheme } from './chart-theme';
import { chartThemeFallback, resolveChartThemeFromCss } from './chart-theme';

type DeepPartial<T> = T extends object ? { [P in keyof T]?: DeepPartial<T[P]> } : T;

function mergeDeep<T>(base: T, override: DeepPartial<T> | undefined): T {
  if (override === undefined) {
    return base;
  }
  if (typeof base !== 'object' || base === null) {
    return (override as T) ?? base;
  }
  const out = { ...base } as Record<string, unknown>;
  const o = override as Record<string, unknown>;
  for (const key of Object.keys(o)) {
    const v = o[key];
    const b = (base as Record<string, unknown>)[key];
    if (v !== undefined && typeof v === 'object' && v !== null && !Array.isArray(v) && typeof b === 'object' && b !== null && !Array.isArray(b)) {
      out[key] = mergeDeep(b as never, v as never);
    } else if (v !== undefined) {
      out[key] = v as never;
    }
  }
  return out as T;
}

function globalDefaultsForTheme(theme: ResolvedChartTheme): ChartOptions {
  return {
    responsive: true,
    maintainAspectRatio: false,
    interaction: { mode: 'nearest', intersect: false },
    font: { family: theme.fontFamily },
    color: theme.textMuted,
    plugins: {
      legend: {
        labels: {
          color: theme.text,
          font: { family: theme.fontFamily, size: 12 },
          boxWidth: 12,
          padding: 16,
        },
      },
      tooltip: {
        backgroundColor: theme.tooltipBg,
        titleColor: theme.tooltipText,
        bodyColor: theme.tooltipText,
        borderColor: theme.tooltipBorder,
        borderWidth: 1,
        padding: 10,
        titleFont: { family: theme.fontFamily, size: 12, weight: 600 },
        bodyFont: { family: theme.fontFamily, size: 12 },
        displayColors: true,
      },
    },
  };
}

function cartesianScalesForTheme(theme: ResolvedChartTheme): NonNullable<ChartOptions['scales']> {
  return {
    x: {
      border: { color: theme.border },
      grid: { color: theme.grid },
      ticks: {
        color: theme.textMuted,
        font: { family: theme.fontFamily, size: 11 },
      },
    },
    y: {
      border: { color: theme.border },
      grid: { color: theme.grid },
      ticks: {
        color: theme.textMuted,
        font: { family: theme.fontFamily, size: 11 },
      },
    },
  };
}

/**
 * Shared defaults for all chart types: fonts, legend, tooltips (no axes — safe for pie/radar overrides).
 * Resolves colors from `--chart-*` CSS variables when `root` is provided (browser).
 */
export function buildChartOptions<TType extends ChartType>(
  overrides?: DeepPartial<ChartOptions<TType>>,
  root?: HTMLElement,
): ChartOptions<TType> {
  const theme = root ? resolveChartThemeFromCss(root) : chartThemeFallback;
  const base = globalDefaultsForTheme(theme) as ChartOptions<TType>;
  return mergeDeep(base, overrides as DeepPartial<ChartOptions<TType>>);
}

/**
 * Line, bar, scatter, etc.: adds axis/grid styling from the design tokens.
 */
export function buildCartesianChartOptions<TType extends ChartType>(
  overrides?: DeepPartial<ChartOptions<TType>>,
  root?: HTMLElement,
): ChartOptions<TType> {
  const theme = root ? resolveChartThemeFromCss(root) : chartThemeFallback;
  const base = {
    ...globalDefaultsForTheme(theme),
    scales: cartesianScalesForTheme(theme),
  } as ChartOptions<TType>;
  return mergeDeep(base, overrides as DeepPartial<ChartOptions<TType>>);
}
