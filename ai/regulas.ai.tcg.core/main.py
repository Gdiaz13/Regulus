"""TCGAI - category (manager) AI for trading-card games.

TCGAI routes each card to the right specialist (PokemonAI, MagicAI,
OnePieceAI, ...), then summarizes and compares them. Specialists that are not
running yet fall back to a local mock with a warning.

Run: uvicorn main:app --port 8202
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.manager import CategoryConfig, SpecialistRef, create_category_app

CONFIG = CategoryConfig(
    model_name="TCGAI",
    model_version="0.1.0",
    category="TCG",
    asset_type="TcgCard",
    specialists={
        "pokemon": SpecialistRef(os.getenv("POKEMON_AI_URL", "http://localhost:8111"), "PokemonAI"),
        "magic": SpecialistRef(os.getenv("MAGIC_AI_URL", "http://localhost:8112"), "MagicAI"),
        "onepiece": SpecialistRef(os.getenv("ONEPIECE_AI_URL", "http://localhost:8113"), "OnePieceAI"),
    },
)

app = create_category_app(CONFIG)
