"""StockTradingAgentsAI - stock research branch inspired by TradingAgents.

This service is intentionally separate from the C# backend and from custom
StockAI sector specialists. It is MOCK for now; the real TradingAgents fork can
replace the adapter internals later without changing the gateway contract.

Run: uvicorn main:app --port 8261
"""

from datetime import date, datetime, timezone
import os
import sys

from fastapi import FastAPI
from pydantic import BaseModel, Field

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from regulas_ai_core.aggregate import build_category
from regulas_ai_core.contract import CategoryPrediction, HealthResponse, ModelInfo, PredictRequest, TrainResponse
from regulas_ai_core.mock import MOCK_WARNING, build_mock_prediction
from regulas_ai_core.training import train_response

MODEL_NAME = "StockTradingAgentsAI"
MODEL_VERSION = "0.1.0"

app = FastAPI(title=MODEL_NAME)


class StockAnalysisRequest(BaseModel):
    symbol: str
    companyName: str = ""
    currentPrice: float = 0.0
    analysisDate: date | None = None


class StockAnalysisResponse(BaseModel):
    symbol: str
    analysisDate: date
    currentPrice: float
    summary: str
    recommendation: str
    confidenceScore: float
    riskScore: float
    bullishArguments: list[str] = Field(default_factory=list)
    bearishArguments: list[str] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)
    rawDecision: dict | None = None
    modelName: str = MODEL_NAME
    modelVersion: str = MODEL_VERSION
    isMock: bool = True
    createdAt: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))


@app.get("/health", response_model=HealthResponse)
def health() -> HealthResponse:
    return HealthResponse(status="ok", modelName=MODEL_NAME, isMock=True)


@app.get("/model-info", response_model=ModelInfo)
def model_info() -> ModelInfo:
    return ModelInfo(
        modelName=MODEL_NAME,
        modelVersion=MODEL_VERSION,
        assetType="Stock",
        category="TradingAgents",
        purpose="Mock stock research engine wrapper for the TradingAgents branch.",
        isMock=True,
    )


@app.post("/analyze-stock", response_model=StockAnalysisResponse)
def analyze_stock(request: StockAnalysisRequest) -> StockAnalysisResponse:
    symbol = request.symbol.strip().upper()
    return _analysis_response(request, symbol)


def _analysis_response(request: StockAnalysisRequest, symbol: str) -> StockAnalysisResponse:
    return StockAnalysisResponse(
        symbol=symbol,
        analysisDate=request.analysisDate or date.today(),
        currentPrice=request.currentPrice,
        summary=f"The mock TradingAgents branch reviewed {symbol} as a research signal.",
        recommendation="research-only hold/watch",
        confidenceScore=0.58,
        riskScore=0.62,
        bullishArguments=["Momentum and narrative support are being watched."],
        bearishArguments=["Volatility and model uncertainty remain elevated."],
        warnings=[MOCK_WARNING, "Research only; not financial advice."],
        rawDecision={"source": "mock-tradingagents-adapter"},
    )


@app.post("/train", response_model=TrainResponse)
def train() -> TrainResponse:
    return train_response(MODEL_NAME, MODEL_VERSION)


@app.post("/explain", response_model=dict)
def explain(request: StockAnalysisRequest) -> dict:
    analysis = analyze_stock(request)
    return {"symbol": analysis.symbol, "summary": analysis.summary, "warnings": analysis.warnings}


@app.post("/predict", response_model=CategoryPrediction)
def predict(requests: list[PredictRequest]) -> CategoryPrediction:
    predictions = [_prediction(item) for item in requests]
    return build_category("TradingAgents", "Stock", predictions, MODEL_NAME, MODEL_VERSION)


def _prediction(request: PredictRequest):
    prediction = build_mock_prediction(request, MODEL_NAME, MODEL_VERSION)
    return prediction.model_copy(update={"rawDecision": {"source": "mock-tradingagents-adapter"}})
