"""FinanceAI - market AI for finance assets.

FinanceAI compares category AIs like StockAI now and CryptoAI later. It is a
MOCK manager layer, but it keeps the hierarchy shaped correctly:
specialist -> category -> market -> RegulasCoreAI.

Run: uvicorn main:app --port 8251
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.manager import CategoryRef, MarketConfig, create_market_app

CONFIG = MarketConfig(
    model_name="FinanceAI",
    model_version="0.1.0",
    market="Finance",
    asset_type="Finance",
    categories={
        "Stock": [
            CategoryRef(os.getenv("STOCK_AI_URL", "http://localhost:8201"), "Stocks", "StockAI"),
            CategoryRef(
                os.getenv("TRADINGAGENTS_STOCK_AI_URL", "http://localhost:8261"),
                "TradingAgents",
                "StockTradingAgentsAI",
            ),
        ],
    },
)

app = create_market_app(CONFIG)
