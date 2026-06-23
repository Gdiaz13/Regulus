"""FastAPI app factories for the manager layers: category AIs and RegulasCoreAI.

Category AIs route each asset to the right specialist and summarize the group.
RegulasCoreAI routes each asset to the right category AI and summarizes those.
Managers only ever talk to the layer directly below them.
"""

from dataclasses import dataclass

from fastapi import FastAPI

from .aggregate import build_category, build_overview
from .contract import CategoryPrediction, HealthResponse, PredictRequest, Prediction, RegulasOverview
from .mock import build_mock_prediction
from .upstream import fetch_category, fetch_prediction


@dataclass(frozen=True)
class SpecialistRef:
    url: str
    model_name: str
    model_version: str = "0.1.0"


@dataclass(frozen=True)
class CategoryConfig:
    model_name: str
    model_version: str
    category: str
    asset_type: str
    specialists: dict[str, SpecialistRef]


@dataclass(frozen=True)
class CategoryRef:
    url: str
    category: str
    model_name: str
    model_version: str = "0.1.0"


@dataclass(frozen=True)
class CommanderConfig:
    model_name: str
    model_version: str
    categories: dict[str, CategoryRef]


def _pick_specialist(config: CategoryConfig, request: PredictRequest) -> SpecialistRef:
    key = request.category.strip().lower()
    return config.specialists.get(key) or next(iter(config.specialists.values()))


def _category_prediction(config: CategoryConfig, request: PredictRequest) -> Prediction:
    ref = _pick_specialist(config, request)
    return fetch_prediction(ref.url, request, ref.model_name, ref.model_version)


def create_category_app(config: CategoryConfig) -> FastAPI:
    """Build a category AI that fans out to its specialists and summarizes them."""
    app = FastAPI(title=config.model_name)
    _register_manager_health(app, config.model_name)
    _register_category_predict(app, config)
    return app


def _register_category_predict(app: FastAPI, config: CategoryConfig) -> None:
    def predict(requests: list[PredictRequest]) -> CategoryPrediction:
        predictions = [_category_prediction(config, item) for item in requests]
        return build_category(config.category, config.asset_type, predictions, config.model_name, config.model_version)

    app.add_api_route("/predict", predict, methods=["POST"], response_model=CategoryPrediction)


def _local_category(ref: CategoryRef, requests: list[PredictRequest]) -> CategoryPrediction:
    predictions = [build_mock_prediction(item, ref.model_name, ref.model_version) for item in requests]
    return build_category(ref.category, requests[0].assetType, predictions, ref.model_name, ref.model_version)


def _commander_category(ref: CategoryRef, requests: list[PredictRequest]) -> CategoryPrediction:
    return fetch_category(ref.url, requests) or _local_category(ref, requests)


def _group_by_type(requests: list[PredictRequest]) -> dict[str, list[PredictRequest]]:
    groups: dict[str, list[PredictRequest]] = {}
    for item in requests:
        groups.setdefault(item.assetType, []).append(item)
    return groups


def create_commander_app(config: CommanderConfig) -> FastAPI:
    """Build RegulasCoreAI: route assets to category AIs, then summarize all of them."""
    app = FastAPI(title=config.model_name)
    _register_manager_health(app, config.model_name)
    _register_commander_predict(app, config)
    return app


def _commander_categories(config: CommanderConfig, requests: list[PredictRequest]) -> list[CategoryPrediction]:
    groups = _group_by_type(requests)
    refs = ((config.categories[asset_type], items) for asset_type, items in groups.items() if asset_type in config.categories)
    return [_commander_category(ref, items) for ref, items in refs]


def _register_commander_predict(app: FastAPI, config: CommanderConfig) -> None:
    def predict(requests: list[PredictRequest]) -> RegulasOverview:
        categories = _commander_categories(config, requests)
        return build_overview(categories, config.model_version)

    app.add_api_route("/predict", predict, methods=["POST"], response_model=RegulasOverview)


def _register_manager_health(app: FastAPI, model_name: str) -> None:
    def health() -> HealthResponse:
        return HealthResponse(status="ok", modelName=model_name, isMock=True)

    app.add_api_route("/health", health, methods=["GET"], response_model=HealthResponse)
