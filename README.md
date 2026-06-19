# Regulus Exchange

This is my stock research and portfolio app. The goal is to keep it useful, readable, and not tangled up: React handles the screens, the .NET API handles data, SQL Server stores the portfolio, and Financial Modeling Prep is the market-data source.

## Quick Map

- `exchange-frontend/` is the React/Vite app.
- `api/` is the .NET API.
- `Exchange.sln` opens the API project in Visual Studio or Rider.

## How It Connects

- The browser loads the React app.
- In local dev, Vite forwards every `/api` request to `http://localhost:5052`.
- Portfolio calls go to `/api/stocks` and use SQL Server through Entity Framework.
- Notes hang off a portfolio stock through `/api/stocks/{stockId}/comments`.
- Market data calls go to `/api/market-data/...`; the API adds the FMP key so the browser never gets it.
- `/api/health` is the quick "is the app wired up?" check for the API, database, and FMP config.

## What You Need

- Node.js and npm
- .NET 9 SDK
- SQL Server LocalDB, SQL Server Express, or another SQL Server instance
- A Financial Modeling Prep API key

## Setup

Set the FMP key on the API side:

```powershell
cd api
dotnet user-secrets set "FinancialModelingPrep:ApiKey" "your_fmp_key"
```

Or use an environment variable:

```powershell
$env:FMP_API_KEY="your_fmp_key"
```

The default database points at SQL Server LocalDB in `api/appsettings.json`. If LocalDB is not running, the API still starts, `/api/health` shows the database as unavailable, and database-backed routes return `503`.

## Run It

Start the API:

```powershell
cd api
dotnet run
```

Start the frontend:

```powershell
cd exchange-frontend
npm.cmd install
npm.cmd run dev
```

## Main Screens

- `/` is the landing page.
- `/search` searches companies and can add them to the portfolio.
- `/portfolio` shows saved stocks and notes.
- `/company/:ticker` opens the company dashboard.
- `/company/:ticker/company-profile` shows key metrics.
- `/company/:ticker/income-statement` shows income statement data.
- `/company/:ticker/balance-sheet` shows balance sheet data.
- `/company/:ticker/cashflow-statement` shows cash flow data.

## Checks I Run

```powershell
cd exchange-frontend
npm.cmd run lint
npm.cmd run lint:functions
npm.cmd run build

cd ..\api
dotnet build --no-restore
```

`npm.cmd run lint:functions` is there on purpose. Keep functions short, focused, and easy to read.

## API Routes

- `GET /api/health`
- `GET /api/stocks`
- `GET /api/stocks/{symbol}`
- `POST /api/stocks`
- `DELETE /api/stocks/{id}`
- `GET /api/stocks/{stockId}/comments`
- `POST /api/stocks/{stockId}/comments`
- `DELETE /api/comments/{id}`
- `GET /api/market-data/{providerPath}`
