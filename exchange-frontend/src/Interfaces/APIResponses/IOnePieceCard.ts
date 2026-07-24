export interface IOnePieceCardSearchResponse {
  cards: IOnePieceCardSummary[];
  page: number;
  pageSize: number;
  count: number;
  totalCount: number;
}

export interface IOnePieceCardSummary {
  id: string;
  name: string;
  setName: string | null;
  code: string | null;
  rarity: string | null;
  color: string | null;
  smallImageUrl: string | null;
  marketPrice: number | null;
  source: string;
  updatedAt: string | null;
}

export interface IOnePieceCardDetail {
  id: string;
  name: string;
  description: string | null;
  setName: string | null;
  code: string | null;
  cardNumber: string | null;
  rarity: string | null;
  color: string | null;
  power: string | null;
  smallImageUrl: string | null;
  largeImageUrl: string | null;
  tcgPlayerUrl: string | null;
  source: string;
  updatedAt: string | null;
  prices: IOnePieceCardPrice[];
}

export interface IOnePieceCardPrice {
  market: string;
  currency: string;
  low: number | null;
  mid: number | null;
  high: number | null;
  marketPrice: number | null;
}
