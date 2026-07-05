// Mirrors the C# price-history DTOs in api/Contracts/PriceHistoryContracts.cs.
export interface IPricePoint {
  date: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  source: string;
  priceType: string | null;
  cardCondition: string | null;
  grade: string | null;
  currency: string | null;
}

export interface IPriceHistory {
  symbol: string;
  assetType: string;
  count: number;
  points: IPricePoint[];
}

export interface ICaptureResult {
  symbol: string;
  assetType: string;
  assetId: number;
  captured: number;
  skipped: number;
  source: string;
}
