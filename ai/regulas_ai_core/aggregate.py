"""Turning many small predictions into one manager-level summary.

Category AIs summarize specialists, market AIs summarize categories, and
RegulasCoreAI summarizes markets. The maths here is intentionally simple
averaging - the point right now is the shape of the data, not clever analysis.
"""

from .contract import CategoryPrediction, Prediction, RegulasOverview


def _average(values: list[float]) -> float:
    return round(sum(values) / len(values), 2) if values else 0.0


def _top_pick(predictions: list[Prediction]) -> Prediction | None:
    return max(predictions, key=lambda item: item.bullishScore, default=None)


def _category_summary(category: str, predictions: list[Prediction]) -> str:
    pick = _top_pick(predictions)
    if pick is None:
        return f"No {category} signals available yet."
    return f"Across {len(predictions)} {category} assets the model leans most bullish on {pick.assetName}."


def build_category(
    category: str,
    asset_type: str,
    predictions: list[Prediction],
    name: str,
    version: str,
    warnings: list[str] | None = None,
) -> CategoryPrediction:
    """Average the specialist predictions and describe the group."""
    return CategoryPrediction(
        category=category,
        assetType=asset_type,
        summary=_category_summary(category, predictions),
        averageConfidence=_average([item.confidenceScore for item in predictions]),
        averageRisk=_average([item.riskScore for item in predictions]),
        predictions=predictions,
        warnings=warnings or [],
        modelName=name,
        modelVersion=version,
    )


def _group_predictions(categories: list[CategoryPrediction]) -> list[Prediction]:
    return [item for category in categories for item in category.predictions]


def _group_warnings(categories: list[CategoryPrediction]) -> list[str]:
    return [item for category in categories for item in category.warnings]


def build_market(
    market: str,
    asset_type: str,
    categories: list[CategoryPrediction],
    name: str,
    version: str,
) -> CategoryPrediction:
    """Combine category AI summaries into one market-level view."""
    return build_category(
        market,
        asset_type,
        _group_predictions(categories),
        name,
        version,
        _group_warnings(categories),
    )


def _overview_summary(categories: list[CategoryPrediction]) -> str:
    if not categories:
        return "No market signals available yet."
    names = ", ".join(item.category for item in categories)
    return f"RegulasCoreAI combined {len(categories)} market views: {names}."


def build_overview(categories: list[CategoryPrediction], version: str) -> RegulasOverview:
    """Combine every market AI into the final Regulas overview."""
    return RegulasOverview(
        summary=_overview_summary(categories),
        categories=categories,
        modelVersion=version,
    )
