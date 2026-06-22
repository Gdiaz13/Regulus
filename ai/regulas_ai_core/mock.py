"""Mock prediction generator.

EVERYTHING in here is fake. There is no real model yet. Numbers are derived
deterministically from the asset id so the same input always returns the same
output, which makes the placeholder easy to test. Real models replace this file
later without touching the service or contract code.
"""

import hashlib

from .contract import PredictRequest, Prediction

MOCK_WARNING = "MOCK DATA - not a real prediction"


def _seed(asset_id: str) -> int:
    digest = hashlib.sha256(asset_id.encode("utf-8")).hexdigest()
    return int(digest[:8], 16)


def _unit(seed: int, offset: int) -> float:
    """A repeatable pseudo-random value between 0 and 1."""
    return ((seed >> offset) % 1000) / 1000.0


def _percent_change(seed: int) -> float:
    """A swing roughly within -15% to +15%."""
    return round((_unit(seed, 0) - 0.5) * 30, 2)


def _scores(seed: int) -> dict[str, float]:
    bullish = round(_unit(seed, 4), 2)
    return {
        "confidenceScore": round(0.4 + _unit(seed, 8) * 0.5, 2),
        "riskScore": round(_unit(seed, 12), 2),
        "bullishScore": bullish,
        "bearishScore": round(1 - bullish, 2),
    }


def _reasons(percent_change: float) -> list[str]:
    trend = "improving" if percent_change >= 0 else "weakening"
    return ["The model estimates sector momentum is " + trend, "Recent price trend looks " + trend]


def build_mock_prediction(request: PredictRequest, model_name: str, model_version: str) -> Prediction:
    """Build a clearly-marked placeholder prediction for one asset."""
    seed = _seed(request.assetId)
    percent_change = _percent_change(seed)
    predicted_price = round(request.currentPrice * (1 + percent_change / 100), 2)
    return _assemble(request, model_name, model_version, percent_change, predicted_price, seed)


def _assemble(
    request: PredictRequest,
    model_name: str,
    model_version: str,
    percent_change: float,
    predicted_price: float,
    seed: int,
) -> Prediction:
    return Prediction(
        assetId=request.assetId,
        assetName=request.assetName or request.assetId,
        assetType=request.assetType,
        category=request.category,
        currentPrice=request.currentPrice,
        predictedPrice=predicted_price,
        predictedPercentChange=percent_change,
        timeHorizonDays=request.timeHorizonDays,
        reasons=_reasons(percent_change),
        warnings=[MOCK_WARNING],
        modelName=model_name,
        modelVersion=model_version,
        **_scores(seed),
    )
