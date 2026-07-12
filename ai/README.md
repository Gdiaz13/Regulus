# Regulas AI Services

This folder holds the Python AI side of Regulas. It is separate from the C# API
on purpose: the backend is the gateway, and these services are the brains it
calls. Right now every brain is a **mock** - the shapes are real, the numbers
are not. Real models drop in later without changing the wiring.

## How the hierarchy works

Predictions flow one way only, from the bottom up:

```
Specialist AI  ->  Category AI  ->  Market AI  ->  RegulasCoreAI  ->  C# gateway
```

- **Specialists** each own one slice: `StockTechAI` and
  `StockSemiconductorAI` for stocks, plus `PokemonAI`, `MagicAI`, and
  `OnePieceAI` for TCG cards. They are the only ones that actually score an
  asset.
- **Category AIs** (`StockAI`, `TCGAI`) route each asset to the right specialist
  and summarize the group.
- `StockAI` accepts common stock category aliases such as `Oil & Gas`,
  `Data Storage`, and `Dividend Growth` and routes them to the energy, memory,
  or dividend specialists instead of falling back to technology.
- **Market AIs** (`FinanceAI`, `CollectiblesAI`) compare category AIs under one
  market. Finance has `StockAI` and `StockTradingAgentsAI` now, plus a crypto
  slot later; Collectibles has `TCGAI` now and room for more categories.
- **RegulasCoreAI** is the commander. It routes each asset to the right market
  AI and returns one combined overview. The C# backend only ever calls this one.

If a service below is not running, the one above falls back to a local mock and
adds a warning, so you can start just one service and still get a response.

## Shared code

`regulas_ai_core/` is the shared library every service imports:

- `contract.py` - the prediction shape. This is the source of truth; the C#
  DTOs in `api/Contracts/AiContracts.cs` mirror it.
- `mock.py` - the fake-but-deterministic prediction generator (clearly marked).
- `training.py` - the mock training response; training stays separate from prediction.
- `service.py` - builds a specialist app from a small config.
- `manager.py` - builds the category, market, and commander apps.
- `aggregate.py` / `upstream.py` - summarizing and calling downstream services.

`regulas.ai.tradingagents.stock/` is the TradingAgents-style stock research
branch. It is a mock adapter right now, not a vendored copy of upstream
TradingAgents. The upstream project is Apache-2.0:
https://github.com/TauricResearch/TradingAgents

## Endpoints

Every AI service exposes the same basic shape:

- `GET /health`
- `GET /model-info`
- `POST /predict`
- `POST /train`
- `POST /explain`

Training is still a mock placeholder. It is separate from prediction on purpose
so real training can become a background job later without changing user
requests.

## Setup

```powershell
cd ai
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

## Run the services

Each runs on its own port. Start the bottom of the tree first if you want real
calls instead of fallbacks:

```powershell
uvicorn main:app --app-dir "regulas.ai.stocks.tech"          --port 8101   # StockTechAI
uvicorn main:app --app-dir "regulas.ai.stocks.semiconductor" --port 8102   # StockSemiconductorAI
uvicorn main:app --app-dir "regulas.ai.stocks.energy"        --port 8103   # StockEnergyAI
uvicorn main:app --app-dir "regulas.ai.stocks.memory"        --port 8104   # StockMemoryAI
uvicorn main:app --app-dir "regulas.ai.stocks.dividend"      --port 8105   # StockDividendAI
uvicorn main:app --app-dir "regulas.ai.tcg.pokemon"          --port 8111   # PokemonAI
uvicorn main:app --app-dir "regulas.ai.tcg.magic"            --port 8112   # MagicAI
uvicorn main:app --app-dir "regulas.ai.tcg.onepiece"         --port 8113   # OnePieceAI
uvicorn main:app --app-dir "regulas.ai.stocks.core"  --port 8201   # StockAI
uvicorn main:app --app-dir "regulas.ai.tcg.core"     --port 8202   # TCGAI
uvicorn main:app --app-dir "regulas.ai.finance.core"      --port 8251   # FinanceAI
uvicorn main:app --app-dir "regulas.ai.collectibles.core" --port 8252   # CollectiblesAI
uvicorn main:app --app-dir "regulas.ai.tradingagents.stock" --port 8261   # StockTradingAgentsAI
uvicorn main:app --app-dir "regulas.ai.core"         --port 8301   # RegulasCoreAI
```

The C# backend points at RegulasCoreAI on `http://localhost:8301` by default
(see `RegulasAi:CoreUrl` in `api/appsettings.json`).

## Tests

```powershell
cd ai
pytest
```

## Ports

| Service          | Port | Status        |
|------------------|------|---------------|
| StockTechAI      | 8101 | mock          |
| StockSemiconductorAI | 8102 | mock      |
| StockEnergyAI    | 8103 | mock          |
| StockMemoryAI    | 8104 | mock          |
| StockDividendAI  | 8105 | mock          |
| PokemonAI        | 8111 | mock          |
| MagicAI          | 8112 | mock          |
| OnePieceAI       | 8113 | mock          |
| StockAI          | 8201 | mock          |
| TCGAI            | 8202 | mock          |
| FinanceAI        | 8251 | mock          |
| CollectiblesAI   | 8252 | mock          |
| StockTradingAgentsAI | 8261 | mock      |
| RegulasCoreAI    | 8301 | mock          |

## Adding a new specialist later

Make a folder like `regulas.ai.stocks.energy`, write a `main.py` with a
`SpecialistConfig`, and register its URL in the matching category AI. Nothing
else has to change. That is the whole point of the hierarchy.
