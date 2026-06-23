"""Shared training placeholder.

Prediction and training stay separate even while training is only a mock.
Real model training can replace this later without touching prediction routes.
"""

from .contract import TrainResponse


def train_response(model_name: str, model_version: str, is_mock: bool = True) -> TrainResponse:
    return TrainResponse(
        status="accepted",
        modelName=model_name,
        modelVersion=model_version,
        message="Mock training placeholder. Real training runs separately later.",
        isMock=is_mock,
    )
