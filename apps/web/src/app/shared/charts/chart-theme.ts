import { chartSeriesPalette } from './chart-palette';

const VAR_PREFIX = '--chart-';

/** Resolved theme for Chart.js (canvas cannot use Tailwind classes). */
export interface ResolvedChartTheme {
  readonly fontFamily: string;
  readonly text: string;
  readonly textMuted: string;
  readonly grid: string;
  readonly border: string;
  readonly tooltipBg: string;
  readonly tooltipText: string;
  readonly tooltipBorder: string;
  readonly series: readonly { border: string; background: string }[];
}

function readCssVar(root: HTMLElement, name: string, fallback: string): string {
  const raw = getComputedStyle(root).getPropertyValue(VAR_PREFIX + name).trim();
  return raw.length > 0 ? raw : fallback;
}

/**
 * Reads `--chart-*` custom properties from `root` (defaults to `document.documentElement`).
 * Safe to call after view init in the browser; use `chartThemeFallback` in non-DOM contexts.
 */
export function resolveChartThemeFromCss(root?: HTMLElement): ResolvedChartTheme {
  if (typeof document === 'undefined') {
    return chartThemeFallback;
  }
  const el = root ?? document.documentElement;
  const series = chartSeriesPalette.map((p, i) => ({
    border: readCssVar(el, `series-${i + 1}-border`, p.border),
    background: readCssVar(el, `series-${i + 1}-fill`, p.background),
  }));
  return {
    fontFamily: readCssVar(el, 'font-family', chartThemeFallback.fontFamily),
    text: readCssVar(el, 'text', chartThemeFallback.text),
    textMuted: readCssVar(el, 'text-muted', chartThemeFallback.textMuted),
    grid: readCssVar(el, 'grid', chartThemeFallback.grid),
    border: readCssVar(el, 'border', chartThemeFallback.border),
    tooltipBg: readCssVar(el, 'tooltip-bg', chartThemeFallback.tooltipBg),
    tooltipText: readCssVar(el, 'tooltip-text', chartThemeFallback.tooltipText),
    tooltipBorder: readCssVar(el, 'tooltip-border', chartThemeFallback.tooltipBorder),
    series,
  };
}

/** Mirrors `:root` defaults in `styles.css` for SSR, tests, and first paint. */
export const chartThemeFallback: ResolvedChartTheme = {
  fontFamily: 'ui-sans-serif, system-ui, sans-serif, "Apple Color Emoji", "Segoe UI Emoji"',
  text: 'rgb(31 41 55)',
  textMuted: 'rgb(75 85 99)',
  grid: 'rgb(226 232 240 / 0.9)',
  border: 'rgb(226 232 240)',
  tooltipBg: 'rgb(255 255 255)',
  tooltipText: 'rgb(17 24 39)',
  tooltipBorder: 'rgb(226 232 240)',
  series: [...chartSeriesPalette],
};
