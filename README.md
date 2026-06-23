# Regulus Exchange

This is my market research and portfolio app. The goal is to keep it useful, readable, and not tangled up: React handles the screens, the .NET API handles data and is the only thing the browser talks to, SQL Server stores the portfolio, Financial Modeling Prep is the market-data source, and a set of small Python AI services handle predictions.

## Quick Map

- `exchange-frontend/` is the React/Vite app.
- `api/` is the .NET API and the gateway for everything else.
- `ai/` is the Python AI services (mock for now). See `ai/README.md`.
- `Exchange.sln` opens the API project in Visual Studio or Rider.

## How It Connects

- The browser loads the React app.
- In local dev, Vite forwards every `/api` request to `http://localhost:5052`.
- Portfolio calls go to `/api/stocks` and use SQL Server through Entity Framework.
- Notes hang off a portfolio stock through `/api/stocks/{stockId}/comments`.
- Market data calls go to `/api/market-data/...`; the API adds the FMP key so the browser never gets it.
- `/api/health` is the quick "is the app wired up?" check for the API, database, and FMP config.
- Prediction calls go to `/api/predict`; the API asks RegulasCoreAI, saves the result, and returns it. The browser never calls the Python services directly.
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

- `Predictions` and `PredictionReasons` store every AI prediction so I can check later which models were actually right. Reasons and warnings live together in `PredictionReasons`, told apart by a `Kind` column, so a saved prediction always carries both the opportunity and the risk side.
- `PriceHistories` stores end-of-day prices (open/high/low/close and volume) per asset, one row per day. This is the real data the models will eventually learn from, and what past predictions get scored against.

The portfolio still runs on the existing `Stocks` table, and there is no generic `Assets` CRUD yet. But capturing price history is the first thing that actually writes `Assets` rows: it find-or-creates the asset for a symbol, so the flexible tables stop being pure groundwork. Everything is built so adding markets and AI later does not mean rewriting the schema.

The `Assets`, `AssetCategories`, `Predictions`, `PredictionReasons`, and `PriceHistories` tables are all created by migrations that run automatically when the API starts in development, so there is nothing to run by hand. The migrations are hand-written raw SQL (see `api/Migrations/`) to keep methods short and the model snapshot in one place.

## The AI Layer

The AI lives in `ai/` as separate Python (FastAPI) services, not inside the API. The backend is the gateway; the AI services are the brains it calls. Everything here is **mock** right now - the data shapes are real, the numbers are fake and clearly flagged. Real models replace the mock generator later without touching the wiring.

Predictions only ever flow upward:

```
Specialist AI  ->  Category AI  ->  RegulasCoreAI  ->  C# gateway  ->  browser
```

- Specialists own one slice each: `StockTechAI`, `PokemonAI`.
- Category AIs (`StockAI`, `TCGAI`) route each asset to the right specialist and summarize them.
- `RegulasCoreAI` is the commander and the only AI the backend calls. It routes each asset to the right category AI and returns one combined overview.

A service falls back to a local mock (with a warning) if the one below it is not running, so I can start just one and still get a response. Full run and port details are in `ai/README.md`. The backend finds RegulasCoreAI through `RegulasAi:CoreUrl` in `api/appsettings.json` (defaults to `http://localhost:8301`), or the `REGULAS_CORE_AI_URL` environment variable.

## Checks I Run

```powershell
cd exchange-frontend
npm.cmd run lint
npm.cmd run lint:functions
npm.cmd run build

cd ..\api
dotnet build --no-restore
dotnet test ..\api.Tests
```

Backend tests live in `api.Tests` (xUnit) and cover the prediction layer (request mapping, saving predictions, the RegulasCoreAI client) and price-history capture (find-or-create asset, dedupe by day). The Python AI services have their own tests - see `ai/README.md`.

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
- `POST /api/price-history/{symbol}/capture` (pull EOD history from FMP, store it, and find-or-create the asset)
- `GET /api/price-history/{symbol}` (read stored history for a symbol)
- `POST /api/predict` (send a list of assets, get back the RegulasCoreAI overview)
- `GET /api/predict/health` (is the AI service reachable?)

## What's Done, Mock, and Planned

- **Done and real:** the portfolio (stocks + notes), market-data proxy, health check, the flexible asset/category tables, the prediction tables, and price-history capture (`/api/price-history` stores EOD prices per asset; needs the FMP key set) shown on a **Prices** page with a simple SVG chart.
- **Done but mock:** the whole AI layer. `POST /api/predict` returns real-shaped predictions with fake numbers, every one flagged with a `MOCK DATA` warning and `IsMock = true` when saved.
- **Planned next:** asset endpoints, more specialists (energy, semiconductor, memory, dividend; magic, one piece), and real models behind the specialists.
- **Crypto** (Bitcoin, Ethereum, etc.) is designed for but not built. The asset types and AI hierarchy already leave room for it.
