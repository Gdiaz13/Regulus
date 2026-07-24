"""Tests for the baseline technical model and its mock fallback wiring."""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi.testclient import TestClient

from regulas_ai_core.baseline import BASELINE_WARNING, build_baseline_prediction, can_use_baseline
from regulas_ai_core.contract import PredictRequest
from regulas_ai_core.service import NO_HISTORY_WARNING, SpecialistConfig, create_specialist_app

RISING = [100 + i for i in range(15)]
FALLING = [114 - i for i in range(15)]
CHOPPY = [100, 108, 96, 110, 94, 112, 92, 114, 90, 116, 88, 118, 86, 120, 100]


def _request(closes: list[float]) -> PredictRequest:
    return PredictRequest(assetId="AMD", assetName="AMD", currentPrice=closes[-1] if closes else 100.0, recentCloses=closes)


def _baseline_client() -> TestClient:
    config = SpecialistConfig("StockTechAI", "0.2.0", "Stock", "Technology", "baseline", is_mock=False, use_baseline=True)
    return TestClient(create_specialist_app(config))


def test_rising_prices_project_a_positive_change_without_mock_warning():
    prediction = build_baseline_prediction(_request(RISING), "StockTechAI", "0.2.0")
    assert prediction.predictedPercentChange > 0
    assert all("MOCK" not in warning for warning in prediction.warnings)
    assert BASELINE_WARNING in prediction.warnings


def test_falling_prices_project_a_negative_change_and_lean_bearish():
    prediction = build_baseline_prediction(_request(FALLING), "StockTechAI", "0.2.0")
    assert prediction.predictedPercentChange < 0
    assert prediction.bearishScore > prediction.bullishScore


def test_choppy_prices_carry_more_risk_and_less_confidence_than_calm_ones():
    calm = build_baseline_prediction(_request(RISING), "StockTechAI", "0.2.0")
    wild = build_baseline_prediction(_request(CHOPPY), "StockTechAI", "0.2.0")
    assert wild.riskScore > calm.riskScore
    assert wild.confidenceScore < calm.confidenceScore


def test_baseline_needs_enough_closes():
    assert not can_use_baseline(_request(RISING[:5]))
    assert can_use_baseline(_request(RISING))


def test_service_uses_baseline_when_closes_are_supplied():
    body = _baseline_client().post("/predict", json=_request(RISING).model_dump()).json()
    assert BASELINE_WARNING in body["warnings"]
    assert all("MOCK" not in warning for warning in body["warnings"])


def test_service_falls_back_to_mock_without_history():
    body = _baseline_client().post("/predict", json=_request([]).model_dump()).json()
    assert any("MOCK" in warning for warning in body["warnings"])
    assert NO_HISTORY_WARNING in body["warnings"]


def test_plain_mock_specialists_are_unchanged():
    config = SpecialistConfig("StockEnergyAI", "0.1.0", "Stock", "Energy", "mock")
    body = TestClient(create_specialist_app(config)).post("/predict", json=_request(RISING).model_dump()).json()
    assert any("MOCK" in warning for warning in body["warnings"])
