"""FastAPI app factory for specialist AI services.

A specialist focuses on one slice of the market (one stock sector, one card
game, etc.). They all share the same endpoints, so each specialist service is
just a config plus this factory. That keeps every specialist tiny and
replaceable, exactly like the project rules ask for.
"""

from dataclasses import dataclass

from fastapi import FastAPI

from .baseline import BASELINE_WARNING, build_baseline_prediction, can_use_baseline
from .contract import HealthResponse, ModelInfo, PredictRequest, Prediction, TrainRequest, TrainResponse
from .mock import build_mock_prediction
from .trainer import train_baseline
from .training import train_response

NO_HISTORY_WARNING = "No stored price history for the baseline model; mock fallback used."


@dataclass(frozen=True)
class SpecialistConfig:
    """Everything that makes one specialist different from another."""

    model_name: str
    model_version: str
    asset_type: str
    category: str
    purpose: str
    is_mock: bool = True
    # Real baseline model (momentum from gateway-supplied closes). Enabled one
    # specialist at a time, exactly as the replace-mocks-last rule asks.
    use_baseline: bool = False


def create_specialist_app(config: SpecialistConfig) -> FastAPI:
    """Build a FastAPI app with the standard specialist endpoints."""
    app = FastAPI(title=config.model_name)
    _register_specialist_routes(app, config)
    return app


def _register_specialist_routes(app: FastAPI, config: SpecialistConfig) -> None:
    app.add_api_route("/health", _health(config), methods=["GET"], response_model=HealthResponse)
    app.add_api_route("/model-info", _model_info(config), methods=["GET"], response_model=ModelInfo)
    app.add_api_route("/predict", _predict(config), methods=["POST"], response_model=Prediction)
    app.add_api_route("/train", _train(config), methods=["POST"], response_model=TrainResponse)
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
        return _build_prediction(config, request)

    return predict


def _build_prediction(config: SpecialistConfig, request: PredictRequest) -> Prediction:
    if config.use_baseline and can_use_baseline(request):
        return build_baseline_prediction(request, config.model_name, config.model_version)
    prediction = build_mock_prediction(request, config.model_name, config.model_version)
    if config.use_baseline:
        prediction.warnings.append(NO_HISTORY_WARNING)
    return prediction


# Baseline specialists train for real from gateway-supplied closes; everyone
# else keeps the clearly-flagged mock placeholder. Training never runs inside
# predict - the backend calls /train from a background job.
def _train(config: SpecialistConfig):
    if config.use_baseline:
        def train_real(request: TrainRequest) -> TrainResponse:
            return train_baseline(request, config.model_name, config.model_version)

        return train_real

    def train() -> TrainResponse:
        return train_response(config.model_name, config.model_version, config.is_mock)

    return train


def _explain(config: SpecialistConfig):
    def explain(request: PredictRequest) -> dict:
        prediction = build_mock_prediction(request, config.model_name, config.model_version)
        return {"assetId": prediction.assetId, "reasons": prediction.reasons, "warnings": prediction.warnings}

    return explain
