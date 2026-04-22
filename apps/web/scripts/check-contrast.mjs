/**
 * WCAG 2.1 contrast checks for the protected UI palette.
 * Colors use Tailwind v4 default theme (oklch) as emitted in dist styles.css.
 */
import 'culori/css';
import { wcagContrast, parse } from 'culori/fn';

/** @type {Record<string, string>} */
const c = {
  white: '#fff',
  // From @layer theme in built styles.css (Tailwind v4 defaults)
  'slate-50': 'oklch(98.4% 0.003 247.858)',
  'slate-100': 'oklch(96.8% 0.007 247.896)',
  'gray-400': 'oklch(70.7% 0.022 261.325)',
  'gray-500': 'oklch(55.1% 0.027 264.364)',
  'gray-600': 'oklch(44.6% 0.03 256.802)',
  'gray-700': 'oklch(37.3% 0.034 259.733)',
  'gray-800': 'oklch(27.8% 0.033 256.848)',
  'gray-900': 'oklch(21% 0.034 264.665)',
  'blue-100': 'oklch(93.2% 0.032 255.585)',
  'blue-600': 'oklch(54.6% 0.245 262.881)',
  'blue-700': 'oklch(48.8% 0.243 264.376)',
  'blue-800': 'oklch(42.4% 0.199 265.638)',
  'blue-900': 'oklch(37.9% 0.146 265.522)',
  'violet-100': 'oklch(94.3% 0.029 294.588)',
  'violet-800': 'oklch(43.2% 0.232 292.759)',
  'rose-50': 'oklch(96.9% 0.015 12.422)',
  'rose-800': 'oklch(45.5% 0.188 13.697)',
  'red-600': 'oklch(57.7% 0.245 27.325)',
};

/** @type {{ label: string; fg: keyof typeof c; bg: keyof typeof c; note?: string }[]} */
const pairs = [
  { label: 'Primary body (gray-900) on page bg (slate-50)', fg: 'gray-900', bg: 'slate-50' },
  { label: 'Shell default text (gray-800) on slate-50', fg: 'gray-800', bg: 'slate-50' },
  { label: 'Secondary text (gray-600) on white (inputs, cards)', fg: 'gray-600', bg: 'white' },
  { label: 'Secondary text (gray-600) on slate-50', fg: 'gray-600', bg: 'slate-50' },
  { label: 'Tertiary / placeholder tone (gray-500) on white', fg: 'gray-500', bg: 'white', note: 'Often used for hints' },
  { label: 'Tertiary (gray-500) on slate-50', fg: 'gray-500', bg: 'slate-50' },
  { label: 'Links / eyebrow (blue-600) on white', fg: 'blue-600', bg: 'white' },
  { label: 'Timeline “message” badge (blue-800) on blue-50-ish panel', fg: 'blue-800', bg: 'slate-50' },
  { label: 'Case status pill: blue-900 on blue-100', fg: 'blue-900', bg: 'blue-100' },
  { label: 'Case priority High: rose tones (rose-800 on rose-50)', fg: 'rose-800', bg: 'rose-50' },
  { label: 'White button label on blue-600', fg: 'white', bg: 'blue-600' },
  { label: 'Table row hover: gray-900 on slate-100', fg: 'gray-900', bg: 'slate-100' },
  { label: 'Event badge (violet-800) on violet-100', fg: 'violet-800', bg: 'violet-100' },
  { label: 'Error text (red-600) on white', fg: 'red-600', bg: 'white' },
  {
    label: 'Chart legend labels (gray-800) on slate-50 card',
    fg: 'gray-800',
    bg: 'slate-50',
    note: 'Legend text in Chart.js; matches --chart-text',
  },
  {
    label: 'Chart tooltip body (gray-900) on white tooltip',
    fg: 'gray-900',
    bg: 'white',
    note: 'Tooltip title/body; matches --chart-tooltip-text on --chart-tooltip-bg',
  },
  {
    label: 'Chart axis tick (gray-600) on slate-50',
    fg: 'gray-600',
    bg: 'slate-50',
    note: 'Category / value ticks over card background',
  },
];

console.log('WCAG 2.1 contrast ratios (culori wcagContrast)\n');
console.log('Thresholds: normal text AA ≥ 4.5:1, AAA ≥ 7:1; large text AA ≥ 3:1, AAA ≥ 4.5:1.\n');

for (const { label, fg, bg, note } of pairs) {
  const ratio = wcagContrast(parse(c[fg]), parse(c[bg]));
  const r = Math.round(ratio * 100) / 100;
  const aa = r >= 4.5 ? '✓' : '✗';
  const aaa = r >= 7 ? '✓' : '✗';
  const largeOk = r >= 3 ? '✓' : '✗';
  const largeAaa = r >= 4.5 ? '✓' : '✗';
  console.log(`${label}`);
  console.log(`  ${fg} on ${bg} → ${r}:1  |  normal AA ${aa}  AAA ${aaa}  |  large AA ${largeOk}  large AAA ${largeAaa}`);
  if (note) console.log(`  Note: ${note}`);
  console.log('');
}
