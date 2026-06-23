"""FastAPI app factory for specialist AI services.

A specialist focuses on one slice of the market (one stock sector, one card
game, etc.). They all share the same endpoints, so each specialist service is
just a config plus this factory. That keeps every specialist tiny and
replaceable, exactly like the project rules ask for.
"""

from dataclasses import dataclass

from fastapi import FastAPI

from .contract import HealthResponse, ModelInfo, PredictRequest, Prediction
from .mock import build_mock_prediction


@dataclass(frozen=True)
class SpecialistConfig:
    """Everything that makes one specialist different from another."""

    model_name: str
    model_version: str
    asset_type: str
    category: str
    purpose: str
    is_mock: bool = True


def create_specialist_app(config: SpecialistConfig) -> FastAPI:
    """Build a FastAPI app with the standard specialist endpoints."""
    app = FastAPI(title=config.model_name)
    _register_specialist_routes(app, config)
    return app


def _register_specialist_routes(app: FastAPI, config: SpecialistConfig) -> None:
    app.add_api_route("/health", _health(config), methods=["GET"], response_model=HealthResponse)
    app.add_api_route("/model-info", _model_info(config), methods=["GET"], response_model=ModelInfo)
    app.add_api_route("/predict", _predict(config), methods=["POST"], response_model=Prediction)
    app.add_api_route("/explain", _explain(config), methods=["POST"], response_model=dict)


def _health(config: SpecialistConfig):
    def health() -> HealthResponse:
        return HealthResponse(status="ok", modelName=config.model_name, isMock=config.is_mock)

    return health


def _model_info(config: SpecialistConfig):
    def model_info() -> ModelInfo:
        return ModelInfo(
            modelName=config.model_name,
            modelVersion=config.model_version,
            assetType=config.asset_type,
            category=config.category,
            purpose=config.purpose,
            isMock=config.is_mock,
        )

    return model_info


def _predict(config: SpecialistConfig):
    def predict(request: PredictRequest) -> Prediction:
        return build_mock_prediction(request, config.model_name, config.model_version)

    return predict


def _explain(config: SpecialistConfig):
    def explain(request: PredictRequest) -> dict:
        prediction = build_mock_prediction(request, config.model_name, config.model_version)
        return {"assetId": prediction.assetId, "reasons": prediction.reasons, "warnings": prediction.warnings}

    return explain
