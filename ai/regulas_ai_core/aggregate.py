"""Turning many small predictions into one manager-level summary.

Category AIs summarize the specialists under them; RegulasCoreAI summarizes the
category AIs under it. The maths here is intentionally simple averaging - the
point right now is the shape of the data, not clever analysis.
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


def build_category(category: str, asset_type: str, predictions: list[Prediction], name: str, version: str) -> CategoryPrediction:
    """Average the specialist predictions and describe the group."""
    return CategoryPrediction(
        category=category,
        assetType=asset_type,
        summary=_category_summary(category, predictions),
        averageConfidence=_average([item.confidenceScore for item in predictions]),
        averageRisk=_average([item.riskScore for item in predictions]),
        predictions=predictions,
        modelName=name,
        modelVersion=version,
    )


def _overview_summary(categories: list[CategoryPrediction]) -> str:
    if not categories:
        return "No category signals available yet."
    names = ", ".join(item.category for item in categories)
    return f"RegulasCoreAI combined {len(categories)} category views: {names}."


def build_overview(categories: list[CategoryPrediction], version: str) -> RegulasOverview:
    """Combine every category AI into the final Regulas overview."""
    return RegulasOverview(
        summary=_overview_summary(categories),
        categories=categories,
        modelVersion=version,
    )
