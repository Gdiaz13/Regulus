"""StockConsumerAI - specialist AI for consumer stocks only.

Covers both consumer discretionary and consumer staples until the two
deserve separate specialists. This is a MOCK placeholder. It returns
structured predictions in the shared contract shape but the numbers are
fake. Real model code replaces the mock generator later without changing
this file.

Run: uvicorn main:app --port 8108
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.service import SpecialistConfig, create_specialist_app

CONFIG = SpecialistConfig(
    model_name="StockConsumerAI",
    model_version="0.1.0",
    asset_type="Stock",
    category="Consumer",
    purpose="Mock price-movement, risk, and opportunity signals for consumer discretionary and staples stocks only.",
)

app = create_specialist_app(CONFIG)
