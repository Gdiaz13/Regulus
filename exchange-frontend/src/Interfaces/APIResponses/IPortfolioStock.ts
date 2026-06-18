export interface IPortfolioStock {
  id: number;
  symbol: string;
  companyName: string;
  purchasePrice: number;
  lastDividend: number;
  industry: string;
  marketCap: number;
}

export interface CreatePortfolioStock {
  symbol: string;
  companyName: string;
  purchasePrice?: number;
  lastDividend?: number;
  industry?: string;
  marketCap?: number;
}
