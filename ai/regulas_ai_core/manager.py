"""FastAPI app factories for the manager layers.

Category AIs route each asset to the right specialist and summarize the group.
Market AIs compare category AIs, and RegulasCoreAI compares market AIs.
Managers only ever talk to the layer directly below them.
"""

from dataclasses import dataclass

from fastapi import FastAPI

from .aggregate import build_category, build_market, build_overview
from .contract import (
    CategoryPrediction,
    HealthResponse,
    ModelInfo,
    PredictRequest,
    Prediction,
    RegulasOverview,
    TrainResponse,
)
from .mock import build_mock_prediction
from .training import train_response
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
class MarketConfig:
    model_name: str
    model_version: str
    market: str
    asset_type: str
    categories: dict[str, list[CategoryRef]]


@dataclass(frozen=True)
class CommanderConfig:
    model_name: str
    model_version: str
    categories: dict[str, CategoryRef]


def _pick_specialist(config: CategoryConfig, request: PredictRequest) -> SpecialistRef:
    key = _specialist_key(request.category)
    return config.specialists.get(key) or next(iter(config.specialists.values()))


def _specialist_key(category: str) -> str:
    return category.strip().lower().replace(" ", "").replace("-", "")


def _category_prediction(config: CategoryConfig, request: PredictRequest) -> Prediction:
    ref = _pick_specialist(config, request)
    return fetch_prediction(ref.url, request, ref.model_name, ref.model_version)


def create_category_app(config: CategoryConfig) -> FastAPI:
    """Build a category AI that fans out to its specialists and summarizes them."""
    app = FastAPI(title=config.model_name)
    _register_category_common(app, config)
    _register_category_predict(app, config)
    return app


def _register_category_predict(app: FastAPI, config: CategoryConfig) -> None:
    def predict(requests: list[PredictRequest]) -> CategoryPrediction:
        predictions = [_category_prediction(config, item) for item in requests]
        return build_category(config.category, config.asset_type, predictions, config.model_name, config.model_version)

    app.add_api_route("/predict", predict, methods=["POST"], response_model=CategoryPrediction)


def _register_category_common(app: FastAPI, config: CategoryConfig) -> None:
    _register_manager_health(app, config.model_name)
    _register_train(app, config.model_name, config.model_version)
    _register_category_info(app, config)
    _register_category_explain(app, config)


def _register_category_info(app: FastAPI, config: CategoryConfig) -> None:
    def model_info() -> ModelInfo:
        return _model_info(config.model_name, config.model_version, config.asset_type, config.category)

    app.add_api_route("/model-info", model_info, methods=["GET"], response_model=ModelInfo)


def _register_category_explain(app: FastAPI, config: CategoryConfig) -> None:
    def explain(requests: list[PredictRequest]) -> dict:
        category = build_category(config.category, config.asset_type, [], config.model_name, config.model_version)
        return {"modelName": config.model_name, "summary": category.summary, "requestCount": len(requests)}

    app.add_api_route("/explain", explain, methods=["POST"], response_model=dict)


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


def create_market_app(config: MarketConfig) -> FastAPI:
    """Build a market AI that compares category AIs below it."""
    app = FastAPI(title=config.model_name)
    _register_market_common(app, config)
    _register_market_predict(app, config)
    return app


def _register_market_predict(app: FastAPI, config: MarketConfig) -> None:
    def predict(requests: list[PredictRequest]) -> CategoryPrediction:
        categories = _market_categories(config, requests)
        return build_market(
            config.market,
            config.asset_type,
            categories,
            config.model_name,
            config.model_version,
        )

    app.add_api_route("/predict", predict, methods=["POST"], response_model=CategoryPrediction)


def _market_categories(config: MarketConfig, requests: list[PredictRequest]) -> list[CategoryPrediction]:
    groups = _group_by_type(requests)
    pairs = _market_pairs(config, groups)
    return [_market_category(ref, items) for ref, items in pairs]


def _market_pairs(config: MarketConfig, groups: dict[str, list[PredictRequest]]):
    return [
        (ref, items)
        for asset_type, items in groups.items()
        for ref in config.categories.get(asset_type, [])
    ]


def _market_category(ref: CategoryRef, requests: list[PredictRequest]) -> CategoryPrediction:
    return fetch_category(ref.url, requests) or _local_category(ref, requests)


def _register_market_common(app: FastAPI, config: MarketConfig) -> None:
    _register_manager_health(app, config.model_name)
    _register_train(app, config.model_name, config.model_version)
    _register_market_info(app, config)
    _register_market_explain(app, config)


def _register_market_info(app: FastAPI, config: MarketConfig) -> None:
    def model_info() -> ModelInfo:
        return _model_info(config.model_name, config.model_version, config.asset_type, config.market)

    app.add_api_route("/model-info", model_info, methods=["GET"], response_model=ModelInfo)


def _register_market_explain(app: FastAPI, config: MarketConfig) -> None:
    def explain(requests: list[PredictRequest]) -> dict:
        return {
            "modelName": config.model_name,
            "summary": "Compares category AIs under one market.",
            "requestCount": len(requests),
        }

    app.add_api_route("/explain", explain, methods=["POST"], response_model=dict)


def create_commander_app(config: CommanderConfig) -> FastAPI:
    """Build RegulasCoreAI: route assets to market AIs, then summarize them."""
    app = FastAPI(title=config.model_name)
    _register_commander_common(app, config)
    _register_commander_predict(app, config)
    return app


def _commander_categories(config: CommanderConfig, requests: list[PredictRequest]) -> list[CategoryPrediction]:
    groups = _group_by_type(requests)
    refs = (
        (config.categories[asset_type], items)
        for asset_type, items in groups.items()
        if asset_type in config.categories
    )
    return [_commander_category(ref, items) for ref, items in refs]


def _register_commander_predict(app: FastAPI, config: CommanderConfig) -> None:
    def predict(requests: list[PredictRequest]) -> RegulasOverview:
        categories = _commander_categories(config, requests)
        return build_overview(categories, config.model_version)

    app.add_api_route("/predict", predict, methods=["POST"], response_model=RegulasOverview)


def _register_commander_common(app: FastAPI, config: CommanderConfig) -> None:
    _register_manager_health(app, config.model_name)
    _register_train(app, config.model_name, config.model_version)
    _register_commander_info(app, config)
    _register_commander_explain(app, config)


def _register_commander_info(app: FastAPI, config: CommanderConfig) -> None:
    def model_info() -> ModelInfo:
        return _model_info(config.model_name, config.model_version, "Mixed", "Core")

    app.add_api_route("/model-info", model_info, methods=["GET"], response_model=ModelInfo)


def _register_commander_explain(app: FastAPI, config: CommanderConfig) -> None:
    def explain(requests: list[PredictRequest]) -> dict:
        return {
            "modelName": config.model_name,
            "summary": "Routes assets upward through market AIs.",
            "requestCount": len(requests),
        }

    app.add_api_route("/explain", explain, methods=["POST"], response_model=dict)


def _register_manager_health(app: FastAPI, model_name: str) -> None:
    def health() -> HealthResponse:
        return HealthResponse(status="ok", modelName=model_name, isMock=True)

    app.add_api_route("/health", health, methods=["GET"], response_model=HealthResponse)


def _register_train(app: FastAPI, model_name: str, model_version: str) -> None:
    def train() -> TrainResponse:
        return train_response(model_name, model_version)

    app.add_api_route("/train", train, methods=["POST"], response_model=TrainResponse)


def _model_info(model_name: str, model_version: str, asset_type: str, category: str) -> ModelInfo:
    return ModelInfo(
        modelName=model_name,
        modelVersion=model_version,
        assetType=asset_type,
        category=category,
        purpose="routes predictions upward through the Regulas AI hierarchy",
        isMock=True,
    )
