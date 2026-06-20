import type {
  CreatePortfolioStock,
  IPortfolioStock,
} from "../Interfaces/APIResponses/IPortfolioStock";
import { apiPath, jsonInit, requestApi } from "./apiClient";
import type { ApiResult } from "./types";

function requestPortfolio<T>(path: string, init?: RequestInit): Promise<ApiResult<T>> {
  return requestApi<T>(apiPath('/api/stocks', path), {
    init: jsonInit(init),
    failureMessage: 'Portfolio request failed',
    connectionMessage: 'Unable to connect to the portfolio API.',
  });
}

export function getPortfolioStocks(): Promise<ApiResult<IPortfolioStock[]>> {
  return requestPortfolio<IPortfolioStock[]>("");
}

export function addPortfolioStock(
  stock: CreatePortfolioStock,
): Promise<ApiResult<IPortfolioStock>> {
  return requestPortfolio<IPortfolioStock>("", {
    method: "POST",
    body: JSON.stringify(stock),
  });
}

export function updatePortfolioStock(
  id: number,
  stock: CreatePortfolioStock,
): Promise<ApiResult<IPortfolioStock>> {
  return requestPortfolio<IPortfolioStock>(`/${id}`, {
    method: "PUT",
    body: JSON.stringify(stock),
  });
}

export function deletePortfolioStock(id: number): Promise<ApiResult<void>> {
  return requestPortfolio<void>(`/${id}`, {
    method: "DELETE",
  });
}
