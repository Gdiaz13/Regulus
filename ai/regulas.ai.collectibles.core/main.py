"""CollectiblesAI - market AI for collectible assets.

CollectiblesAI compares category AIs like TCGAI now and future collectible
categories later. It is a MOCK manager layer, but it keeps the hierarchy shaped
correctly: specialist -> category -> market -> RegulasCoreAI.

Run: uvicorn main:app --port 8252
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.manager import CategoryRef, MarketConfig, create_market_app

CONFIG = MarketConfig(
    model_name="CollectiblesAI",
    model_version="0.1.0",
    market="Collectibles",
    asset_type="Collectibles",
    categories={
        "TcgCard": [CategoryRef(os.getenv("TCG_AI_URL", "http://localhost:8202"), "TCG", "TCGAI")],
    },
)

app = create_market_app(CONFIG)
