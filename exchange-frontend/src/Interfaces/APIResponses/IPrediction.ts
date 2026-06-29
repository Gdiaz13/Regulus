// These mirror the AI contract returned by the C# gateway (/api/predict),
// which in turn mirrors ai/regulas_ai_core/contract.py. Keep the three in sync.

export interface IPredictAsset {
  symbol: string;
  name?: string;
  assetType: string;
  category?: string;
  currentPrice: number;
  timeHorizonDays?: number;
}

export interface IAiPrediction {
  assetId: string;
  assetName: string;
  assetType: string;
  category: string;
  currentPrice: number;
  predictedPrice: number;
  predictedPercentChange: number;
  confidenceScore: number;
  riskScore: number;
  bullishScore: number;
  bearishScore: number;
  timeHorizonDays: number;
  reasons: string[];
  warnings: string[];
  rawDecision: unknown | null;
  modelName: string;
  modelVersion: string;
  createdAt: string;
}

export interface IAiCategoryPrediction {
  category: string;
  assetType: string;
  summary: string;
  averageConfidence: number;
  averageRisk: number;
  predictions: IAiPrediction[];
  warnings: string[];
  modelName: string;
  modelVersion: string;
  createdAt: string;
}

export interface IAiOverview {
  summary: string;
  categories: IAiCategoryPrediction[];
  modelName: string;
  modelVersion: string;
  createdAt: string;
}

export interface IPredictionHealth {
  aiAvailable: boolean;
}

export interface IPredictionHistoryItem {
  id: number;
  assetId: string;
  assetName: string;
  assetType: string;
  category: string;
  currentPrice: number;
  predictedPrice: number;
  predictedPercentChange: number;
  confidenceScore: number;
  riskScore: number;
  bullishScore: number;
  bearishScore: number;
  timeHorizonDays: number;
  reasons: string[];
  warnings: string[];
  modelName: string;
  modelVersion: string;
  isMock: boolean;
  createdOn: string;
}
