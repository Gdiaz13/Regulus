"""Calling downstream services from one AI manager to the next layer.

The hierarchy only ever flows one way: a manager calls the services directly
beneath it. To keep development easy, if a downstream service is not running the
call falls back to a local mock and adds a warning, so a single service still
works on its own.
"""

import httpx

from .contract import CategoryPrediction, PredictRequest, Prediction
from .mock import build_mock_prediction

OFFLINE_WARNING = "downstream service offline - used local mock"


def fetch_prediction(url: str, request: PredictRequest, fallback_model: str, version: str) -> Prediction:
    """Ask a specialist for a prediction; fall back to a local mock if it is down."""
    try:
        return _post_prediction(url, request)
    except (httpx.HTTPError, ValueError):
        return _offline_prediction(request, fallback_model, version)


def _post_prediction(url: str, request: PredictRequest) -> Prediction:
    with httpx.Client(timeout=5.0) as client:
        response = client.post(f"{url}/predict", json=request.model_dump())
        response.raise_for_status()
        return Prediction(**response.json())


def _offline_prediction(request: PredictRequest, fallback_model: str, version: str) -> Prediction:
    prediction = build_mock_prediction(request, fallback_model, version)
    prediction.warnings.append(OFFLINE_WARNING)
    return prediction


def fetch_category(url: str, requests: list[PredictRequest]) -> CategoryPrediction | None:
    """Ask a category AI for its summary; return None if it is unreachable."""
    try:
        return _post_category(url, requests)
    except (httpx.HTTPError, ValueError):
        return None


def _post_category(url: str, requests: list[PredictRequest]) -> CategoryPrediction:
    payload = [item.model_dump() for item in requests]
    with httpx.Client(timeout=5.0) as client:
        response = client.post(f"{url}/predict", json=payload)
        response.raise_for_status()
        return CategoryPrediction(**response.json())
