"""Tests for the specialist, category, and commander FastAPI apps.

Downstream services are not running during tests, so the managers exercise their
local-mock fallback path. That is on purpose: the hierarchy must still work when
only one service is up.
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi.testclient import TestClient

from regulas_ai_core.manager import (
    CategoryConfig,
    CategoryRef,
    CommanderConfig,
    SpecialistRef,
    create_category_app,
    create_commander_app,
)
from regulas_ai_core.service import SpecialistConfig, create_specialist_app

STOCK_REQUEST = {"assetId": "AMD", "assetName": "AMD", "assetType": "Stock", "category": "Technology", "currentPrice": 100.0}


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


def _commander_client() -> TestClient:
    config = CommanderConfig(
        "RegulasCoreAI", "0.1.0",
        {"Stock": CategoryRef("http://localhost:8201", "Stocks", "StockAI")},
    )
    return TestClient(create_commander_app(config))


def test_commander_returns_overview():
    body = _commander_client().post("/predict", json=[STOCK_REQUEST]).json()
    assert body["modelName"] == "RegulasCoreAI" and len(body["categories"]) == 1
