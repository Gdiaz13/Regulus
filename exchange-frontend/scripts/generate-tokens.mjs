import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

// Emits src/styles/tokens.generated.css from the shared design/design-tokens.json so the
// web app and the future MAUI app read their colors from one source. Output is
// deterministic (no timestamps) so it can be committed without noisy diffs.
const here = dirname(fileURLToPath(import.meta.url));
const tokensPath = resolve(here, '../../design/design-tokens.json');
const outPath = resolve(here, '../src/styles/tokens.generated.css');

// Keep this order and these names identical to the old hand-written index.css block.
const order = ['background', 'foreground', 'card', 'primary', 'primaryForeground', 'border', 'accent', 'portfolioCardBg'];
const cssName = {
  background: '--background', foreground: '--foreground', card: '--card', primary: '--primary',
  primaryForeground: '--primary-foreground', border: '--border', accent: '--accent', portfolioCardBg: '--portfolio-card-bg',
};

function declarations(palette) {
  return order.map((key) => `    ${cssName[key]}: ${palette[key].hsl ?? palette[key].hex};`).join('\n');
}

function ruleBlock(selector, palette) {
  return `  ${selector} {\n${declarations(palette)}\n  }`;
}

function generate() {
  const tokens = JSON.parse(readFileSync(tokensPath, 'utf8'));
  const head = '/* Generated from design/design-tokens.json by scripts/generate-tokens.mjs. Do not edit by hand. */';
  const body = `@layer base {\n${ruleBlock(':root', tokens.color.light)}\n${ruleBlock('.dark', tokens.color.dark)}\n}\n`;
  mkdirSync(dirname(outPath), { recursive: true });
  writeFileSync(outPath, `${head}\n${body}`);
}

generate();
