"""Tests for the shared contract and the mock generator."""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.contract import Prediction, PredictRequest
from regulas_ai_core.mock import MOCK_WARNING, build_mock_prediction

PREDICTION_FIELDS = {
    "assetId", "assetName", "assetType", "category", "currentPrice", "predictedPrice",
    "predictedPercentChange", "confidenceScore", "riskScore", "bullishScore",
    "bearishScore", "timeHorizonDays", "reasons", "warnings", "modelName",
    "modelVersion", "createdAt",
}


def _request() -> PredictRequest:
    return PredictRequest(assetId="AMD", assetName="Advanced Micro Devices", assetType="Stock", category="Technology", currentPrice=100.0)


def test_prediction_has_every_contract_field():
    prediction = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    assert PREDICTION_FIELDS.issubset(prediction.model_dump().keys())


def test_mock_is_deterministic():
    first = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    second = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    assert first.predictedPrice == second.predictedPrice


def test_mock_is_clearly_marked():
    prediction = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    assert MOCK_WARNING in prediction.warnings


def test_scores_stay_in_range():
    prediction = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    assert 0.0 <= prediction.confidenceScore <= 1.0
    assert 0.0 <= prediction.riskScore <= 1.0
    assert round(prediction.bullishScore + prediction.bearishScore, 2) == 1.0


def test_predicted_price_follows_percent_change():
    prediction = build_mock_prediction(_request(), "StockTechAI", "0.1.0")
    expected = round(100.0 * (1 + prediction.predictedPercentChange / 100), 2)
    assert prediction.predictedPrice == expected
