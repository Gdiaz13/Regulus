export interface IStockTradingAgentsRequest {
  symbol: string;
  companyName?: string;
  currentPrice: number;
  analysisDate?: string;
}

export interface IStockTradingAgentsResponse {
  symbol: string;
  analysisDate: string;
  currentPrice: number;
  summary: string;
  recommendation: string;
  confidenceScore: number;
  riskScore: number;
  bullishArguments: string[];
  bearishArguments: string[];
  warnings: string[];
  rawDecision: unknown | null;
  modelName: string;
  modelVersion: string;
  isMock: boolean;
  createdAt: string;
}

export interface ITradingAgentsHealth {
  aiAvailable: boolean;
}

export interface ITradingAgentsModelInfo {
  modelName: string;
  modelVersion: string;
  assetType: string;
  category: string;
  purpose: string;
  isMock: boolean;
}
