"""Shared building blocks for every Regulas AI service.

Specialist, category, and commander services all import from here so the
prediction contract, mock data, and FastAPI wiring stay in one place. Keeping
this shared keeps each service tiny and replaceable.
"""

from .contract import (
    CategoryPrediction,
    HealthResponse,
    ModelInfo,
    Prediction,
    PredictRequest,
    RegulasOverview,
)

__all__ = [
    "CategoryPrediction",
    "HealthResponse",
    "ModelInfo",
    "Prediction",
    "PredictRequest",
    "RegulasOverview",
]
