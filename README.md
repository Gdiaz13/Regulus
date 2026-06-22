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
- Portfolio symbols are stored uppercase, capped at 32 characters, and kept unique in the database.

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

## Data Model Groundwork

Regulas is meant to track more than stocks down the road (ETFs, TCG cards, and crypto later), so the database now has a flexible foundation sitting next to the portfolio tables:

- `Assets` holds anything trackable, tagged with an `AssetType` (`Stock`, `Etf`, `TcgCard`, `Crypto`, or `Collectible`). The same symbol can live under different types, so a stock ticker and a card code never collide.
- `AssetCategories` groups assets into market segments like "Technology" or "Pokemon". These line up with the category-level AI layer that comes later.

This is groundwork only. There are no `Assets` endpoints yet and the portfolio still runs on the existing `Stocks` table. The new tables are here so that adding markets and AI predictions later does not mean rewriting the schema.

## Checks I Run

```powershell
cd exchange-frontend
npm.cmd run lint
npm.cmd run lint:functions
npm.cmd run build

cd ..\api
dotnet build --no-restore
```

`npm.cmd run lint:functions` is there on purpose. It checks the frontend, frontend scripts, and API code so functions stay short, focused, and easy to read.

## API Routes

- `GET /api/health`
- `GET /api/stocks`
- `GET /api/stocks/{symbol}`
- `POST /api/stocks`
- `PUT /api/stocks/{id}`
- `DELETE /api/stocks/{id}`
- `GET /api/stocks/{stockId}/comments`
- `POST /api/stocks/{stockId}/comments`
- `PUT /api/comments/{id}`
- `DELETE /api/comments/{id}`
- `GET /api/market-data/{providerPath}`
