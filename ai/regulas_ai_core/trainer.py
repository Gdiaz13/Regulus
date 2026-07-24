"""Walk-forward trainer for the momentum baseline.

It fits one honest number: the damping that maps a 10-close momentum window
to the next move. Each symbol's samples split chronologically into
train/validation/test segments, windows only ever look backward, and nothing
shuffles - so there is no look-ahead leakage. The chosen damping is judged on
the untouched test segment against the untrained default, and the caller
records both errors so only measured improvements get promoted later.
"""

import math

from .contract import TrainRequest, TrainResponse, TrainSeries

TRAINING_CONTRACT_VERSION = "1.0"
WINDOW = 10
DEFAULT_DAMPING = 0.5
CANDIDATE_DAMPINGS = (0.25, 0.5, 0.75, 1.0)
MIN_SAMPLES = 20
MIN_TEST_SAMPLES = 3
PROXY_WARNING = "Trained on next-close targets as a proxy; horizon-length targets need more stored history."


def train_baseline(request: TrainRequest, model_name: str, model_version: str) -> TrainResponse:
    train, validation, test = _split_samples(request.series)
    total = len(train) + len(validation) + len(test)
    if total < MIN_SAMPLES or len(test) < MIN_TEST_SAMPLES:
        return _insufficient(model_name, model_version, total)
    damping = _select_damping(validation or train)
    return _trained_response(model_name, model_version, damping, (train, validation, test))


def _split_samples(series_list: list[TrainSeries]) -> tuple[list, list, list]:
    """Per-symbol chronological 60/20/20 split, then pooled across symbols."""
    train, validation, test = [], [], []
    for series in series_list:
        samples = _series_samples(series.closes)
        first, second = int(len(samples) * 0.6), int(len(samples) * 0.8)
        train += samples[:first]
        validation += samples[first:second]
        test += samples[second:]
    return train, validation, test


def _series_samples(closes: list[float]) -> list[tuple[float, float]]:
    """(window momentum %, next-close change %) pairs; windows look backward only."""
    samples = []
    for index in range(WINDOW - 1, len(closes) - 1):
        sample = _sample_at(closes, index)
        if sample is not None:
            samples.append(sample)
    return samples


# Stored data can hold zeros or garbage; bad points drop out instead of
# poisoning every error metric with NaN.
def _sample_at(closes: list[float], index: int) -> tuple[float, float] | None:
    window_start = closes[index - WINDOW + 1]
    if window_start <= 0 or closes[index] <= 0:
        return None
    momentum = _change_percent(window_start, closes[index])
    target = _change_percent(closes[index], closes[index + 1])
    return (momentum, target) if math.isfinite(momentum) and math.isfinite(target) else None


def _change_percent(first: float, last: float) -> float:
    return (last - first) / first * 100.0


def _select_damping(validation: list[tuple[float, float]]) -> float:
    """Grid-select on validation error; ties fall back toward the default."""
    return min(CANDIDATE_DAMPINGS, key=lambda damping: (_mae(validation, damping), abs(damping - DEFAULT_DAMPING)))


def _mae(samples: list[tuple[float, float]], damping: float) -> float:
    if not samples:
        return float("inf")
    return sum(abs(momentum * damping - target) for momentum, target in samples) / len(samples)


def _trained_response(model_name: str, model_version: str, damping: float, splits: tuple) -> TrainResponse:
    train, validation, test = splits
    test_mae, baseline_mae = _mae(test, damping), _mae(test, DEFAULT_DAMPING)
    metrics = {
        "trainSamples": len(train), "validationSamples": len(validation), "testSamples": len(test),
        "testMae": round(test_mae, 6), "baselineMae": round(baseline_mae, 6),
        "improved": test_mae <= baseline_mae,
    }
    return TrainResponse(
        status="completed", modelName=model_name, modelVersion=model_version,
        message=f"Fitted momentum damping {damping}; test MAE {metrics['testMae']} vs default {metrics['baselineMae']}.",
        isMock=False, trained=True, metrics=metrics, warnings=[PROXY_WARNING],
        artifact={"damping": damping, "window": WINDOW, "method": "walk-forward-momentum-v1"},
    )


def _insufficient(model_name: str, model_version: str, count: int) -> TrainResponse:
    return TrainResponse(
        status="insufficient-data", modelName=model_name, modelVersion=model_version,
        message=f"Only {count} training sample(s) from stored closes; need at least {MIN_SAMPLES}.",
        isMock=False, trained=False, warnings=[PROXY_WARNING],
    )
