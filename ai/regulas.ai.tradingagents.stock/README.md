# StockTradingAgentsAI

This is the Regulas stock research branch for TradingAgents-style analysis.

It is a mock adapter right now. No upstream TradingAgents code is vendored in
this folder yet. The real fork/package can replace the adapter internals later
while keeping this FastAPI boundary:

- `GET /health`
- `GET /model-info`
- `POST /analyze-stock`
- `POST /predict`
- `POST /train`
- `POST /explain`

Upstream project: https://github.com/TauricResearch/TradingAgents

License note: TradingAgents is Apache-2.0. If the forked code is added later,
keep the Apache-2.0 license and attribution with the forked package.

TradingAgents is used as research support only. It is not financial advice.
