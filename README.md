# Regulus Exchange

Regulus Exchange is a full-stack stock research and portfolio app. The frontend is a React/Vite app, and the backend is a .NET API with Entity Framework models for persisted portfolio stocks and comments.

## Project Structure

- `exchange-frontend/`: React, TypeScript, Vite, and CSS modules.
- `api/`: .NET API, EF Core context, migrations, and portfolio endpoints.
- `Exchange.sln`: solution file for the API project.

## Prerequisites

- Node.js and npm
- .NET 9 SDK
- SQL Server Express or another SQL Server instance
- Financial Modeling Prep API key

## Configuration

Store the Financial Modeling Prep key in API configuration. For local development, user secrets or an environment variable keep it out of source control:

```powershell
cd api
dotnet user-secrets set "FinancialModelingPrep:ApiKey" "your_fmp_key"
```

PowerShell environment variable alternative:

```powershell
$env:FMP_API_KEY="your_fmp_key"
```

Update `api/appsettings.json` if your SQL Server connection string is different from the local default.

## Run Locally

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

The frontend dev server proxies `/api` calls to `http://localhost:5052`. Market-data requests go through the API so the FMP key is not shipped to the browser.

## Main Routes

- `/`: landing page
- `/search`: company search and quick portfolio actions
- `/portfolio`: persisted portfolio management
- `/company/:ticker`: company dashboard
- `/company/:ticker/company-profile`: key metrics
- `/company/:ticker/income-statement`: income statement
- `/company/:ticker/balance-sheet`: balance sheet
- `/company/:ticker/cashflow-statement`: cash flow statement

## Verification

```powershell
cd exchange-frontend
npm.cmd run lint
npm.cmd run build

cd ..\api
dotnet build --no-restore
```

Code should stay modular: keep functions short, focused, typed, and easy to verify.
