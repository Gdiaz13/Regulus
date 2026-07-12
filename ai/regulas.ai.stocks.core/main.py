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

TECH = SpecialistRef(os.getenv("STOCK_TECH_AI_URL", "http://localhost:8101"), "StockTechAI")
SEMI = SpecialistRef(os.getenv("STOCK_SEMI_AI_URL", "http://localhost:8102"), "StockSemiconductorAI")
ENERGY = SpecialistRef(os.getenv("STOCK_ENERGY_AI_URL", "http://localhost:8103"), "StockEnergyAI")
MEMORY = SpecialistRef(os.getenv("STOCK_MEMORY_AI_URL", "http://localhost:8104"), "StockMemoryAI")
DIVIDEND = SpecialistRef(os.getenv("STOCK_DIVIDEND_AI_URL", "http://localhost:8105"), "StockDividendAI")

CONFIG = CategoryConfig(
    model_name="StockAI",
    model_version="0.1.0",
    category="Stocks",
    asset_type="Stock",
    specialists={
        "technology": TECH,
        "informationtechnology": TECH,
        "semiconductor": SEMI,
        "semiconductors": SEMI,
        "energy": ENERGY,
        "oil&gas": ENERGY,
        "oilandgas": ENERGY,
        "memory": MEMORY,
        "datastorage": MEMORY,
        "storage": MEMORY,
        "dividend": DIVIDEND,
        "dividendgrowth": DIVIDEND,
        "income": DIVIDEND,
    },
)

app = create_category_app(CONFIG)
