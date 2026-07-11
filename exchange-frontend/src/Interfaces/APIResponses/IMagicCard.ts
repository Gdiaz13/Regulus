export interface IMagicCardSearchResponse {
  cards: IMagicCardSummary[];
  page: number;
  pageSize: number;
  count: number;
  totalCount: number;
}

export interface IMagicCardSummary {
  id: string;
  name: string;
  setName: string | null;
  setCode: string | null;
  collectorNumber: string | null;
  rarity: string | null;
  smallImageUrl: string | null;
  marketPrice: number | null;
  marketCurrency: string | null;
  source: string;
  updatedAt: string | null;
}

export interface IMagicCardDetail extends IMagicCardSummary {
  typeLine: string | null;
  manaCost: string | null;
  oracleText: string | null;
  colors: string[];
  artist: string | null;
  largeImageUrl: string | null;
  scryfallUrl: string | null;
  prices: IMagicCardPrice[];
}

export interface IMagicCardPrice {
  currency: string;
  finish: string;
  marketPrice: number;
}
