# Regulas design system

This is the shared look for Regulas. Both frontends — the Vite web app (`Regulas.WebApp`)
and the future installed app (`Regulas.MauiApp`) — should feel like the same product, so
the colors live in one place: `design/design-tokens.json`. The web app is the visual
reference; if a color changes in the web app, change it here too so MAUI can match.

## The feel

Regulas has a calm, slightly cosmic feel — a deep night background, a clear blue as the
main color, and a gold accent for the things that matter (links on hover, the chart line,
the symbol on a card). Light mode is a soft lavender-white; dark mode is near-black.

## Colors

Stored as HSL components (what the web CSS variables use) plus a hex equivalent (what MAUI
will use). `--primary` and `--accent` are the same in both themes on purpose.

| Token | Meaning | Light | Dark |
|---|---|---|---|
| primary | main brand blue (links, buttons, focus) | `#4c88ff` | `#4c88ff` |
| accent | gold highlight (hover, chart line, symbols) | `#ffcc00` | `#ffcc00` |
| background | page background | `#edebfa` | `#05080f` |
| foreground | main text | `#000000` | `#e1e7ef` |
| card | surface / card background | `#4c88ff` | `#0b111e` |
| border | borders, dividers | `#364563` | `#222f44` |
| primaryForeground | text on primary | `#fffff5` | `#fffff5` |
| portfolioCardBg | portfolio card surface | `#ede9fe` | `#181926` |

Semantic (not part of the light/dark palette — same in both): success `#2ecc71`,
warning `#ffb347`, danger `#e74c3c`. Muted text = foreground at ~60% opacity.
Chart: gold line, success/danger for up/down.

## Shape & spacing

Radius: sm `0.5rem`, md `0.75rem`, lg `1rem`, pill `999px`. Cards use a subtle surface
(`foreground` at ~2-4% opacity), a soft border (`border` at ~50%), and a light hover lift
with a `primary` glow. Page width caps at `min(1080px, 100% - 2rem)`.

## How each frontend uses this

- **Web:** `exchange-frontend/scripts/generate-tokens.mjs` reads this file and writes
  `src/styles/tokens.generated.css` (the `:root` / `.dark` CSS variables). `index.css`
  imports it. The generator runs automatically on `npm run dev` / `npm run build`, or
  manually with `npm run tokens`. Don't hand-edit the generated file.
- **MAUI (later):** map the hex values above into MAUI `Color` resources (light + dark)
  so the installed app matches the web app without copying colors by hand.
