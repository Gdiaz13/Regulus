"""StockAI - category (manager) AI for stocks.

StockAI does not predict on its own. It routes each stock to the right
specialist (TechAI, SemiconductorAI, ...), then summarizes and compares them.
Specialists that are not running yet fall back to a local mock with a warning,
so the structure works before every specialist exists.

Run: uvicorn main:app --port 8201
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.manager import CategoryConfig, SpecialistRef, create_category_app

CONFIG = CategoryConfig(
    model_name="StockAI",
    model_version="0.1.0",
    category="Stocks",
    asset_type="Stock",
    specialists={
        "technology": SpecialistRef(os.getenv("STOCK_TECH_AI_URL", "http://localhost:8101"), "StockTechAI"),
        "semiconductor": SpecialistRef(os.getenv("STOCK_SEMI_AI_URL", "http://localhost:8102"), "StockSemiconductorAI"),
    },
)

app = create_category_app(CONFIG)
