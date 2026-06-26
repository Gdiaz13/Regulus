# Regulas Exchange

Regulas is my market research and portfolio app. The point is not to make one giant magic AI box. The point is clean data flow first: the web app talks to the C# API, the API owns data and secrets, PostgreSQL stores the durable market data, and separate Python AI services return research signals that can be saved and checked later.

The current app is still mid-migration. The web app and API run, the AI services are mock but structured, and the active backend store now uses PostgreSQL through Dapper.

## Quick Map

- `exchange-frontend/` is the React/Vite web app.
- `api/` is the .NET API and the gateway for data, market providers, and AI services.
- `api/Database/Migrations/` is the new plain-SQL PostgreSQL migration path.
- `ai/` is the separate Python AI service workspace. See `ai/README.md`.
- `design/` holds shared design tokens for the web app and future MAUI app.
- `Exchange.sln` opens the C# projects in Visual Studio or Rider.

## How It Connects

```text
Regulas.WebApp -> Regulas.Api -> PostgreSQL
Regulas.WebApp -> Regulas.Api -> RegulasCoreAI -> Market AIs -> Category AIs -> Specialist AIs
Regulas.WebApp -> Regulas.Api -> StockTradingAgentsAI
```

No frontend should call Python services, TradingAgents, PostgreSQL, or external provider APIs directly. The backend is the gateway for API keys, persistence, errors, and clean response contracts.

## Current Stack

- Web frontend: Vite, React, TypeScript
- Backend: ASP.NET Core on .NET 10
- Database: PostgreSQL
- Database access: Dapper with plain SQL migrations
- AI services: Python FastAPI services, mock data for now
- TradingAgents: separate stock research service boundary

## Shared Design

Colors live in `design/design-tokens.json`. The web app generates CSS variables from that file with:

```powershell
cd exchange-frontend
npm.cmd run tokens
```

`npm.cmd run dev` and `npm.cmd run build` also regenerate the tokens. The future `Regulas.MauiApp` should use the same values so the installed app feels like the same product, not a separate skin.

## Setup

Set the Financial Modeling Prep key on the API side:

```powershell
cd api
dotnet user-secrets set "FinancialModelingPrep:ApiKey" "your_fmp_key"
```

Or use an environment variable:

```powershell
$env:FMP_API_KEY="your_fmp_key"
```

The default PostgreSQL connection is:

```text
Host=localhost;Port=5432;Database=regulas;Username=regulas;Password=regulas
```

Override it with:

```powershell
$env:DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=regulas;Username=regulas;Password=regulas"
```

Or set the individual pieces:

```powershell
$env:POSTGRES_HOST="localhost"
$env:POSTGRES_PORT="5432"
$env:POSTGRES_DB="regulas"
$env:POSTGRES_USER="regulas"
$env:POSTGRES_PASSWORD="regulas"
```

`DATABASE_CONNECTION_STRING` wins if both styles are set. `.env.example` has the same values for local reference.

Start local PostgreSQL with Docker Compose:

```powershell
docker compose up -d postgres
```

The API runs `api/Database/Migrations/*.sql` at startup when PostgreSQL is available. If PostgreSQL is offline, startup logs a warning and keeps going. `/api/health` now probes PostgreSQL through the Dapper/Npgsql path.

## Run It

Start the API:

```powershell
cd api
dotnet run
```

Start the web app:

```powershell
cd exchange-frontend
npm.cmd install
npm.cmd run dev
```

Run the mock RegulasCoreAI service:

```powershell
cd ai
.\.venv\Scripts\python.exe -m uvicorn main:app --app-dir "regulas.ai.core" --reload --port 8301
```

Run the mock StockTradingAgentsAI service:

```powershell
cd ai
.\.venv\Scripts\python.exe -m uvicorn main:app --app-dir "regulas.ai.tradingagents.stock" --reload --port 8261
```

## Main Screens

- `/` is the landing page.
- `/search` searches companies and can add them to the portfolio.
- `/portfolio` shows saved stocks and notes.
- `/predictions` stages assets and asks the mock AI hierarchy for research signals.
- `/trading-agents` runs stock TradingAgents research through the C# gateway.
- `/price-history` captures and reads stored price history.
- `/company/:ticker` opens the company dashboard.
- `/company/:ticker/company-profile` shows key metrics.
- `/company/:ticker/income-statement` shows income statement data.
- `/company/:ticker/balance-sheet` shows balance sheet data.
- `/company/:ticker/cashflow-statement` shows cash flow data.

## Data Model

Regulas needs to track more than stocks. The schema is shaped around flexible assets:

- `assets` holds stocks, ETFs, TCG cards, crypto, and collectibles through Dapper.
- `asset_categories` groups assets into markets or segments like Technology or Pokemon through Dapper.
- `price_history` stores end-of-day prices and source metadata through Dapper.
- `predictions` stores every AI prediction so accuracy can be checked later.
- `prediction_reasons` stores reasons and warnings separately from the main row.
- `stocks` and `comments` keep the current portfolio feature working through Dapper.

The first PostgreSQL baseline is `api/Database/Migrations/001_initial_regulas_schema.sql`. It includes source fields such as provider, external asset id, source timestamp, collection time, and raw provider reference where useful. The API runs these SQL files at startup and records applied migrations in `regulas_schema_migrations`.

## AI Layer

The AI lives outside the C# backend as separate Python services. The backend calls the services and returns clean responses to the frontend.

Predictions flow upward:

```text
Specialist AI -> Category AI -> Market AI -> RegulasCoreAI -> C# API -> frontend
```

Current mock services include:

- `StockTechAI`
- `StockSemiconductorAI`
- `PokemonAI`
- `MagicAI`
- `OnePieceAI`
- `StockAI`
- `TCGAI`
- `FinanceAI`
- `CollectiblesAI`
- `RegulasCoreAI`
- `StockTradingAgentsAI`

`StockTradingAgentsAI` is a separate research branch inspired by TauricResearch TradingAgents. It is not copied into the C# backend and should stay replaceable behind its API. It is research only, not financial advice.

## API Routes

- `GET /api/health`
- `GET /api/assets`
- `GET /api/assets?assetType=Stock`
- `GET /api/assets/{id}`
- `POST /api/assets`
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
- `POST /api/price-history/{symbol}/capture`
- `GET /api/price-history/{symbol}`
- `POST /api/predict`
- `GET /api/predict/history`
- `GET /api/predict/accuracy`
- `GET /api/predict/health`
- `POST /api/trading-agents/stock/analyze`
- `GET /api/trading-agents/stock/health`
- `GET /api/trading-agents/stock/model-info`

## Checks

```powershell
cd exchange-frontend
npm.cmd run lint
npm.cmd run lint:functions
npm.cmd run build

cd ..\api
dotnet build --no-restore
dotnet test ..\api.Tests

cd ..\ai
.\.venv\Scripts\python.exe -m pytest
```

## Done, Mock, And Planned

Done and real:

- Web app screens for search, portfolio, prices, predictions, and TradingAgents research.
- C# backend gateway for market data, portfolio routes, assets, prices, predictions, and TradingAgents.
- Shared design tokens for web and future MAUI styling.
- PostgreSQL/Dapper migration foundation, local compose setup, and PostgreSQL health probe.
- Flexible assets, price-history capture/read, portfolio stocks, and stock notes now use PostgreSQL/Dapper behind the existing API contracts.

Done but mock:

- AI hierarchy from specialists up to RegulasCoreAI.
- StockTradingAgentsAI adapter.
- Prediction values, confidence, risk, reasons, and warnings.

Still planned:

- Add `Regulas.MauiApp` as the installed app frontend.
- Add background jobs for price snapshots, prediction scoring, and training.
- Add more stock specialists, TCG detail screens, and future crypto support.
- Replace mock AI internals with real models once the data flow is solid.
