"""PokemonAI - specialist AI for Pokemon TCG cards only.

This is a MOCK placeholder. It returns structured predictions in the shared
contract shape but the numbers are fake. Real model code replaces the mock
generator later without changing this file.

Run: uvicorn main:app --port 8111
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.service import SpecialistConfig, create_specialist_app

CONFIG = SpecialistConfig(
    model_name="PokemonAI",
    model_version="0.1.0",
    asset_type="TcgCard",
    category="Pokemon",
    purpose="Mock price-movement, risk, and opportunity signals for Pokemon cards only.",
)

app = create_specialist_app(CONFIG)
