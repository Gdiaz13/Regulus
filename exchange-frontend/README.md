# Regulus Frontend Notes

This is the browser side of the app. It renders the pages, talks to `/api`, and leaves secrets and database work to the .NET API.

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

## How The Frontend Connects

- `src/Routes/Routes.tsx` decides which page renders for each URL.
- `src/API/*Client.ts` files are the only place components should call `fetch`.
- `src/hooks/usePortfolioStocks.ts` connects portfolio screens to `/api/stocks`.
- `src/Components/Portfolio/StockDetails` owns the portfolio detail edit form.
- `src/hooks/useStockComments.ts` connects stock notes to `/api/stocks/{stockId}/comments`.
- `src/hooks/useTickerResource.ts` keeps the company statement pages using the same loading/error pattern.
- `src/Components/HealthStatus` reads `/api/health` for the navbar status pill.
- Keep components and helpers small: one job per function, no long render bodies.
