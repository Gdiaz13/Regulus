# Regulus Frontend

React/Vite frontend for the Regulus Exchange app.

## Setup

Install dependencies:

```powershell
npm.cmd install
```

## Scripts

```powershell
npm.cmd run dev
npm.cmd run lint
npm.cmd run lint:functions
npm.cmd run build
npm.cmd run preview
```

The Vite dev server proxies `/api` requests to the .NET API at `http://localhost:5052`.
Configure the Financial Modeling Prep key on the API, not in the frontend.

## Notes

- Shared API results use `ApiResult<T>`.
- Portfolio state is shared through `src/hooks/usePortfolioStocks.ts`.
- Portfolio notes are shared through `src/hooks/useStockComments.ts`.
- Company statement pages use `src/hooks/useTickerResource.ts`.
- Keep components and helpers small: one job per function, no long render bodies.
