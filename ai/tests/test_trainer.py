"""Tests for the walk-forward momentum trainer and the /train wiring."""

import math

from fastapi.testclient import TestClient

from regulas_ai_core.contract import TrainRequest, TrainSeries
from regulas_ai_core.service import SpecialistConfig, create_specialist_app
from regulas_ai_core.trainer import (
    CANDIDATE_DAMPINGS,
    DEFAULT_DAMPING,
    WINDOW,
    _mae,
    _select_damping,
    _series_samples,
    train_baseline,
)


def _wavy_closes(count: int = 130) -> list[float]:
    """Bounded oscillation with drift: realistic momenta, always positive."""
    return [100.0 + 6.0 * math.sin(i / 3.0) + 0.05 * i for i in range(count)]


def _request(closes: list[float]) -> TrainRequest:
    return TrainRequest(series=[TrainSeries(symbol="AAPL", closes=closes)])


def test_selection_picks_the_damping_that_generated_the_targets():
    momenta = (1.0, -2.0, 3.5, 0.5, -1.5, 2.0, -0.75, 4.0)
    samples = [(momentum, 0.25 * momentum) for momentum in momenta]
    assert _select_damping(samples) == 0.25
    assert _mae(samples, 0.25) < _mae(samples, DEFAULT_DAMPING)


def test_trainer_fits_and_reports_honest_metrics():
    response = train_baseline(_request(_wavy_closes()), "StockTechAI", "0.2.0")
    assert response.trained is True and response.status == "completed"
    assert response.artifact is not None and response.artifact["damping"] in CANDIDATE_DAMPINGS
    metrics = response.metrics
    assert metrics is not None and metrics["testSamples"] >= 3
    assert math.isfinite(metrics["testMae"]) and math.isfinite(metrics["baselineMae"])
    assert isinstance(metrics["improved"], bool)
    assert response.isMock is False and response.contractVersion == "1.0"


def test_trainer_needs_enough_samples():
    response = train_baseline(_request(_wavy_closes(count=WINDOW + 6)), "StockTechAI", "0.2.0")
    assert response.trained is False and response.status == "insufficient-data"
    assert "need at least" in response.message


def test_samples_only_look_backward():
    closes = [float(100 + i) for i in range(WINDOW + 3)]
    samples = _series_samples(closes)
    assert len(samples) == 3
    first_momentum, first_target = samples[0]
    assert first_momentum == (closes[WINDOW - 1] - closes[0]) / closes[0] * 100.0
    assert first_target == (closes[WINDOW] - closes[WINDOW - 1]) / closes[WINDOW - 1] * 100.0


def test_bad_points_drop_out_instead_of_poisoning_metrics():
    closes = _wavy_closes(40)
    closes[20] = 0.0
    samples = _series_samples(closes)
    assert all(math.isfinite(momentum) and math.isfinite(target) for momentum, target in samples)


def test_baseline_specialist_trains_over_http():
    config = SpecialistConfig("StockTechAI", "0.2.0", "Stock", "Technology", "test", use_baseline=True)
    client = TestClient(create_specialist_app(config))
    body = client.post("/train", json={"series": [{"symbol": "AAPL", "closes": _wavy_closes()}]}).json()
    assert body["trained"] is True and body["isMock"] is False
    assert body["artifact"]["damping"] in CANDIDATE_DAMPINGS
    assert body["metrics"]["testSamples"] >= 3


def test_mock_specialist_train_stays_a_placeholder():
    config = SpecialistConfig("StockSemiconductorAI", "0.1.0", "Stock", "Semiconductors", "test")
    client = TestClient(create_specialist_app(config))
    body = client.post("/train").json()
    assert body["isMock"] is True and body["trained"] is False
    assert "placeholder" in body["message"]


def test_default_damping_is_a_candidate():
    assert DEFAULT_DAMPING in CANDIDATE_DAMPINGS
