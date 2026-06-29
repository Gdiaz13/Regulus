"""RegulasCoreAI - the commander at the top of the hierarchy.

It does not predict on its own. It routes each asset to the right market AI
(FinanceAI for stocks, CollectiblesAI for cards, CryptoAI later through
FinanceAI), collects their summaries, and returns one combined overview. This
is the single AI entry point the C# gateway calls.

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
        "Stock": CategoryRef(os.getenv("FINANCE_AI_URL", "http://localhost:8251"), "Finance", "FinanceAI"),
        "Crypto": CategoryRef(os.getenv("FINANCE_AI_URL", "http://localhost:8251"), "Finance", "FinanceAI"),
        "TcgCard": CategoryRef(os.getenv("COLLECTIBLES_AI_URL", "http://localhost:8252"), "Collectibles", "CollectiblesAI"),
        "Collectible": CategoryRef(os.getenv("COLLECTIBLES_AI_URL", "http://localhost:8252"), "Collectibles", "CollectiblesAI"),
    },
)

app = create_commander_app(CONFIG)
