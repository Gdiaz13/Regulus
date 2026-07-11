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

// What the manual capture endpoint accepts (TCG cards first), including the game category.
export interface IManualPriceRequest {
  date: string;
  price: number;
  priceType?: string | null;
  cardCondition?: string | null;
  grade?: string | null;
  currency?: string | null;
  name?: string | null;
  category?: string | null;
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
