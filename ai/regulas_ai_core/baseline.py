"""Baseline technical model: real numbers from real stored prices.

This is the first non-mock model in the hierarchy. It is deliberately simple -
momentum and volatility computed from the gateway-supplied closes - so its
accuracy can be judged honestly by the scoring pipeline before anything
fancier replaces it. No training step: the computation is deterministic.
"""

import statistics

from .contract import PredictRequest, Prediction

MIN_CLOSES = 10
MAX_PROJECTED_CHANGE = 20.0
BASELINE_WARNING = "Baseline technical model - momentum and volatility only, not financial advice."


def can_use_baseline(request: PredictRequest) -> bool:
    """The baseline needs enough stored closes and a usable current price."""
    return len(request.recentCloses) >= MIN_CLOSES and request.currentPrice > 0


def build_baseline_prediction(request: PredictRequest, model_name: str, model_version: str) -> Prediction:
    """Momentum-projected estimate with volatility-driven risk and confidence."""
    closes = request.recentCloses
    momentum = _window_change_percent(closes)
    volatility = _daily_volatility_percent(closes)
    percent_change = _projected_change(momentum)
    return _assemble(request, model_name, model_version, percent_change, momentum, volatility)


def _window_change_percent(closes: list[float]) -> float:
    """Percent change across the stored window (oldest to newest close)."""
    first, last = closes[0], closes[-1]
    return 0.0 if first == 0 else round((last - first) / first * 100, 2)


def _daily_volatility_percent(closes: list[float]) -> float:
    returns = [_daily_return(closes, index) for index in range(1, len(closes))]
    return round(statistics.pstdev(returns) * 100, 2) if returns else 0.0


def _daily_return(closes: list[float], index: int) -> float:
    previous = closes[index - 1]
    return 0.0 if previous == 0 else (closes[index] - previous) / previous


def _projected_change(momentum: float) -> float:
    """Project half the observed momentum forward, capped to stay humble."""
    projected = momentum / 2
    return round(max(-MAX_PROJECTED_CHANGE, min(MAX_PROJECTED_CHANGE, projected)), 2)


def _scores(momentum: float, volatility: float) -> dict[str, float]:
    bullish = _bullish(momentum)
    return {
        "confidenceScore": _confidence(volatility),
        "riskScore": round(min(1.0, volatility / 5), 2),
        "bullishScore": bullish,
        "bearishScore": round(1 - bullish, 2),
    }


def _confidence(volatility: float) -> float:
    """Calmer price action earns more confidence, within honest bounds."""
    return round(max(0.35, min(0.9, 0.9 - volatility / 10)), 2)


def _bullish(momentum: float) -> float:
    """Map momentum in roughly -20..+20 onto a 0..1 bullish score."""
    return round(max(0.0, min(1.0, 0.5 + momentum / 40)), 2)


def _reasons(momentum: float, volatility: float, window: int) -> list[str]:
    direction = "up" if momentum >= 0 else "down"
    return [
        f"The model estimates from stored prices: {window}-point trend is {direction} {abs(momentum)}%",
        f"Daily volatility over the window is {volatility}%",
    ]


def _assemble(
    request: PredictRequest,
    model_name: str,
    model_version: str,
    percent_change: float,
    momentum: float,
    volatility: float,
) -> Prediction:
    return Prediction(
        assetId=request.assetId,
        assetName=request.assetName or request.assetId,
        assetType=request.assetType,
        category=request.category,
        currentPrice=request.currentPrice,
        predictedPrice=round(request.currentPrice * (1 + percent_change / 100), 2),
        predictedPercentChange=percent_change,
        timeHorizonDays=request.timeHorizonDays,
        reasons=_reasons(momentum, volatility, len(request.recentCloses)),
        warnings=[BASELINE_WARNING],
        modelName=model_name,
        modelVersion=model_version,
        **_scores(momentum, volatility),
    )
