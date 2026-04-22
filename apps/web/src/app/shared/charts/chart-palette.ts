/**
 * Fallback palette when CSS variables are unavailable (SSR/tests). Values align with `styles.css`
 * `--chart-series-*` and Tailwind-style blues / violets / emeralds used in the app shell.
 */
export const chartSeriesPalette = [
  { border: 'rgb(37 99 235)', background: 'rgb(37 99 235 / 0.12)' },
  { border: 'rgb(124 58 237)', background: 'rgb(124 58 237 / 0.12)' },
  { border: 'rgb(5 150 105)', background: 'rgb(5 150 105 / 0.12)' },
  { border: 'rgb(217 119 6)', background: 'rgb(217 119 6 / 0.14)' },
  { border: 'rgb(219 39 119)', background: 'rgb(219 39 119 / 0.12)' },
  { border: 'rgb(79 70 229)', background: 'rgb(79 70 229 / 0.12)' },
] as const;

export function chartSeriesStyle(index: number): (typeof chartSeriesPalette)[number] {
  return chartSeriesPalette[index % chartSeriesPalette.length]!;
}
