import type {
  CreatePortfolioStock,
  IPortfolioStock,
} from "../Interfaces/APIResponses/IPortfolioStock";
import type { ApiResult } from "./types";

async function requestPortfolio<T>(
  path: string,
  init?: RequestInit,
): Promise<ApiResult<T>> {
  try {
    const response = await fetch(`/api/stocks${path}`, {
      headers: {
        "Content-Type": "application/json",
        ...init?.headers,
      },
      ...init,
    });

    if (!response.ok) {
      const message = await response.text();
      return {
        ok: false,
        message: message || `Portfolio request failed with ${response.status}.`,
      };
    }

    if (response.status === 204) {
      return {
        ok: true,
        data: undefined as T,
      };
    }

    return {
      ok: true,
      data: (await response.json()) as T,
    };
  } catch {
    return {
      ok: false,
      message: "Unable to connect to the portfolio API.",
    };
  }
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

export function deletePortfolioStock(id: number): Promise<ApiResult<void>> {
  return requestPortfolio<void>(`/${id}`, {
    method: "DELETE",
  });
}
