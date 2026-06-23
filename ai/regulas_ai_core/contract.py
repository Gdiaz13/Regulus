"""The shared API contract for every Regulas AI service.

These models are the single source of truth for what a prediction looks like.
The C# gateway mirrors these shapes in its DTOs, so changing a field here means
updating the gateway too.
"""

from datetime import datetime, timezone

from pydantic import BaseModel, Field


def _utc_now() -> datetime:
    return datetime.now(timezone.utc)


class PredictRequest(BaseModel):
    """What the gateway sends when it wants a prediction for one asset."""

    assetId: str
    assetName: str = ""
    assetType: str = "Stock"
    category: str = ""
    currentPrice: float = 0.0
    timeHorizonDays: int = 90


class Prediction(BaseModel):
    """A single specialist prediction. Matches the README contract exactly."""

    assetId: str
    assetName: str
    assetType: str
    category: str
    currentPrice: float
    predictedPrice: float
    predictedPercentChange: float
    confidenceScore: float
    riskScore: float
    bullishScore: float
    bearishScore: float
    timeHorizonDays: int
    reasons: list[str] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)
    modelName: str
    modelVersion: str
    createdAt: datetime = Field(default_factory=_utc_now)


class CategoryPrediction(BaseModel):
    """A category AI's summary that compares the specialists beneath it."""

    category: str
    assetType: str
    summary: str
    averageConfidence: float
    averageRisk: float
    predictions: list[Prediction] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)
    modelName: str
    modelVersion: str
    createdAt: datetime = Field(default_factory=_utc_now)


class RegulasOverview(BaseModel):
    """RegulasCoreAI's final overview across every category AI."""

    summary: str
    categories: list[CategoryPrediction] = Field(default_factory=list)
    modelName: str = "RegulasCoreAI"
    modelVersion: str
    createdAt: datetime = Field(default_factory=_utc_now)


class ModelInfo(BaseModel):
    """Answer for GET /model-info so callers know what they are talking to."""

    modelName: str
    modelVersion: str
    assetType: str
    category: str
    purpose: str
    isMock: bool


class HealthResponse(BaseModel):
    """Answer for GET /health."""

    status: str
    modelName: str
    isMock: bool
