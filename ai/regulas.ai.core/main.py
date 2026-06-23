"""RegulasCoreAI - the commander at the top of the hierarchy.

It does not predict on its own. It routes each asset to the right category AI
(StockAI for stocks, TCGAI for cards, CryptoAI later), collects their summaries,
and returns one combined overview. This is the single AI entry point the C#
gateway calls.

Run: uvicorn main:app --port 8301
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.manager import CategoryRef, CommanderConfig, create_commander_app

CONFIG = CommanderConfig(
    model_name="RegulasCoreAI",
    model_version="0.1.0",
    categories={
        "Stock": CategoryRef(os.getenv("STOCK_AI_URL", "http://localhost:8201"), "Stocks", "StockAI"),
        "TcgCard": CategoryRef(os.getenv("TCG_AI_URL", "http://localhost:8202"), "TCG", "TCGAI"),
    },
)

app = create_commander_app(CONFIG)
