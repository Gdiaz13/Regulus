# Regulus Frontend

React/Vite frontend for the Regulus Exchange app.

## Setup

Create `.env` in this folder:

```ini
VITE_EXCHANGE_KEY=your_fmp_key
```

Install dependencies:

```powershell
npm.cmd install
```

## Scripts

```powershell
npm.cmd run dev
npm.cmd run lint
npm.cmd run build
npm.cmd run preview
```

The Vite dev server proxies `/api` requests to the .NET API at `http://localhost:5052`.

## Notes

- Shared API results use `ApiResult<T>`.
- Portfolio state is shared through `src/hooks/usePortfolioStocks.ts`.
- Company statement pages use `src/hooks/useTickerResource.ts`.
- Keep components and helpers small: one job per function, no long render bodies.
