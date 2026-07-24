"""StockTechAI - specialist AI for technology stocks only.

The first specialist with a REAL model: a baseline that computes momentum and
volatility from stored closes the gateway supplies. When an asset has no
stored history yet, it falls back to the clearly-marked mock with a warning.

Run: uvicorn main:app --port 8101
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.service import SpecialistConfig, create_specialist_app

CONFIG = SpecialistConfig(
    model_name="StockTechAI",
    model_version="0.2.0",
    asset_type="Stock",
    category="Technology",
    purpose="Baseline momentum and volatility signals for technology stocks, computed from stored prices.",
    is_mock=False,
    use_baseline=True,
)

app = create_specialist_app(CONFIG)
