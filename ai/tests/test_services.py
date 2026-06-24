"""Tests for the specialist, category, and commander FastAPI apps.

Downstream services are not running during tests, so the managers exercise their
local-mock fallback path. That is on purpose: the hierarchy must still work when
only one service is up.
"""

import os
import sys
import importlib.util
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi.testclient import TestClient

from regulas_ai_core.manager import (
    CategoryConfig,
    CategoryRef,
    CommanderConfig,
    MarketConfig,
    SpecialistRef,
    create_category_app,
    create_commander_app,
    create_market_app,
)
from regulas_ai_core.service import SpecialistConfig, create_specialist_app

AI_ROOT = Path(__file__).resolve().parents[1]
STOCK_REQUEST = {"assetId": "AMD", "assetName": "AMD", "assetType": "Stock", "category": "Technology", "currentPrice": 100.0}
TCG_REQUEST = {"assetId": "OP-01", "assetName": "Starter Card", "assetType": "TcgCard", "category": "Pokemon", "currentPrice": 12.0}


def _specialist_client() -> TestClient:
    config = SpecialistConfig("StockTechAI", "0.1.0", "Stock", "Technology", "tech stocks")
    return TestClient(create_specialist_app(config))


def test_specialist_health_reports_mock():
    body = _specialist_client().get("/health").json()
    assert body["status"] == "ok" and body["isMock"] is True


def test_specialist_model_info():
    body = _specialist_client().get("/model-info").json()
    assert body["modelName"] == "StockTechAI" and body["assetType"] == "Stock"


def test_specialist_predict_returns_contract():
    body = _specialist_client().post("/predict", json=STOCK_REQUEST).json()
    assert body["assetId"] == "AMD" and body["modelName"] == "StockTechAI"


def test_specialist_train_is_mock_placeholder():
    body = _specialist_client().post("/train").json()
    assert body["status"] == "accepted" and body["isMock"] is True


def _category_client() -> TestClient:
    config = CategoryConfig(
        "StockAI", "0.1.0", "Stocks", "Stock",
        {"technology": SpecialistRef("http://localhost:8101", "StockTechAI")},
    )
    return TestClient(create_category_app(config))


def test_category_summarizes_specialists():
    body = _category_client().post("/predict", json=[STOCK_REQUEST]).json()
    assert body["category"] == "Stocks" and len(body["predictions"]) == 1
    assert "offline" in body["predictions"][0]["warnings"][-1]


def test_category_has_standard_manager_routes():
    client = _category_client()
    assert client.get("/model-info").json()["modelName"] == "StockAI"
    assert client.post("/train").json()["status"] == "accepted"
    assert client.post("/explain", json=[STOCK_REQUEST]).json()["requestCount"] == 1


def _multi_specialist_client() -> TestClient:
    config = CategoryConfig(
        "StockAI", "0.1.0", "Stocks", "Stock",
        {
            "technology": SpecialistRef("http://localhost:8101", "StockTechAI"),
            "semiconductor": SpecialistRef("http://localhost:8102", "StockSemiconductorAI"),
        },
    )
    return TestClient(create_category_app(config))


def test_category_routes_to_semiconductor_specialist():
    request = {**STOCK_REQUEST, "assetId": "AMD", "category": "Semiconductor"}
    body = _multi_specialist_client().post("/predict", json=[request]).json()
    assert body["predictions"][0]["modelName"] == "StockSemiconductorAI"


def _tcg_client() -> TestClient:
    config = CategoryConfig(
        "TCGAI", "0.1.0", "TCG", "TcgCard",
        {
            "pokemon": SpecialistRef("http://localhost:8111", "PokemonAI"),
            "magic": SpecialistRef("http://localhost:8112", "MagicAI"),
            "onepiece": SpecialistRef("http://localhost:8113", "OnePieceAI"),
        },
    )
    return TestClient(create_category_app(config))


def test_tcg_category_routes_to_magic_specialist():
    request = {**TCG_REQUEST, "assetId": "MTG-01", "category": "Magic"}
    body = _tcg_client().post("/predict", json=[request]).json()
    assert body["predictions"][0]["modelName"] == "MagicAI"


def test_tcg_category_routes_one_piece_with_spaces():
    request = {**TCG_REQUEST, "assetId": "OP-01", "category": "One Piece"}
    body = _tcg_client().post("/predict", json=[request]).json()
    assert body["predictions"][0]["modelName"] == "OnePieceAI"


def _load_service(folder: str):
    path = AI_ROOT / folder / "main.py"
    spec = importlib.util.spec_from_file_location(folder, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def test_magic_service_config_is_loadable():
    module = _load_service("regulas.ai.tcg.magic")
    assert module.CONFIG.model_name == "MagicAI"


def test_one_piece_service_config_is_loadable():
    module = _load_service("regulas.ai.tcg.onepiece")
    assert module.CONFIG.category == "One Piece"


def _market_client() -> TestClient:
    config = MarketConfig(
        "FinanceAI", "0.1.0", "Finance", "Finance",
        {"Stock": [CategoryRef("http://localhost:8201", "Stocks", "StockAI")]},
    )
    return TestClient(create_market_app(config))


def test_market_ai_summarizes_category_ai_results():
    body = _market_client().post("/predict", json=[STOCK_REQUEST]).json()
    assert body["category"] == "Finance" and body["modelName"] == "FinanceAI"
    assert body["predictions"][0]["modelName"] == "StockAI"


def test_market_ai_has_standard_manager_routes():
    client = _market_client()
    assert client.get("/model-info").json()["modelName"] == "FinanceAI"
    assert client.post("/train").json()["status"] == "accepted"
    assert client.post("/explain", json=[STOCK_REQUEST]).json()["requestCount"] == 1


def test_market_ai_compares_multiple_branches_for_one_asset_type():
    config = MarketConfig(
        "FinanceAI", "0.1.0", "Finance", "Finance",
        {"Stock": [
            CategoryRef("http://localhost:8201", "Stocks", "StockAI"),
            CategoryRef("http://localhost:8261", "TradingAgents", "StockTradingAgentsAI"),
        ]},
    )
    body = TestClient(create_market_app(config)).post("/predict", json=[STOCK_REQUEST]).json()
    assert [item["modelName"] for item in body["predictions"]] == ["StockAI", "StockTradingAgentsAI"]


def test_finance_service_config_is_loadable():
    module = _load_service("regulas.ai.finance.core")
    assert module.CONFIG.model_name == "FinanceAI"


def test_collectibles_service_config_is_loadable():
    module = _load_service("regulas.ai.collectibles.core")
    assert module.CONFIG.market == "Collectibles"


def test_stock_tradingagents_service_analyzes_stock():
    module = _load_service("regulas.ai.tradingagents.stock")
    body = TestClient(module.app).post("/analyze-stock", json={"symbol": "amd"}).json()
    assert body["symbol"] == "AMD" and body["isMock"] is True


def test_stock_tradingagents_has_standard_support_routes():
    module = _load_service("regulas.ai.tradingagents.stock")
    client = TestClient(module.app)
    assert client.post("/train").json()["status"] == "accepted"
    assert client.post("/explain", json={"symbol": "amd"}).json()["symbol"] == "AMD"


def test_stock_tradingagents_predict_includes_raw_decision():
    module = _load_service("regulas.ai.tradingagents.stock")
    body = TestClient(module.app).post("/predict", json=[STOCK_REQUEST]).json()
    assert body["predictions"][0]["rawDecision"]["source"] == "mock-tradingagents-adapter"


def _commander_client() -> TestClient:
    config = CommanderConfig(
        "RegulasCoreAI", "0.1.0",
        {"Stock": CategoryRef("http://localhost:8251", "Finance", "FinanceAI")},
    )
    return TestClient(create_commander_app(config))


def test_commander_returns_overview():
    body = _commander_client().post("/predict", json=[STOCK_REQUEST]).json()
    assert body["modelName"] == "RegulasCoreAI" and len(body["categories"]) == 1
    assert body["categories"][0]["category"] == "Finance"


def test_commander_has_standard_manager_routes():
    client = _commander_client()
    assert client.get("/model-info").json()["category"] == "Core"
    assert client.post("/train").json()["isMock"] is True
    assert client.post("/explain", json=[STOCK_REQUEST]).json()["requestCount"] == 1
