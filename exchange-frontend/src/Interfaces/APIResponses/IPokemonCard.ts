export interface IPokemonCardSearchResponse {
  cards: IPokemonCardSummary[];
  page: number;
  pageSize: number;
  count: number;
  totalCount: number;
}

export interface IPokemonCardSummary {
  id: string;
  name: string;
  setName: string | null;
  setSeries: string | null;
  number: string | null;
  rarity: string | null;
  smallImageUrl: string | null;
  marketPrice: number | null;
  source: string;
  updatedAt: string | null;
}

export interface IPokemonCardDetail extends IPokemonCardSummary {
  supertype: string | null;
  subtypes: string[];
  hp: string | null;
  types: string[];
  artist: string | null;
  largeImageUrl: string | null;
  tcgPlayerUrl: string | null;
  prices: IPokemonCardPrice[];
}

export interface IPokemonCardPrice {
  variant: string;
  low: number | null;
  mid: number | null;
  high: number | null;
  market: number | null;
  directLow: number | null;
}
