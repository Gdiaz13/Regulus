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
    const response = await fetch(`/api/stocks${path}`, portfolioInit(init));
    return readPortfolioResponse(response);
  } catch {
    return portfolioFailure("Unable to connect to the portfolio API.");
  }
}

function portfolioInit(init?: RequestInit): RequestInit {
  return { ...init, headers: portfolioHeaders(init) };
}

function portfolioHeaders(init?: RequestInit) {
  const headers = new Headers(init?.headers);
  headers.set("Content-Type", "application/json");
  return headers;
}

async function readPortfolioResponse<T>(response: Response): Promise<ApiResult<T>> {
  if (!response.ok) {
    return portfolioFailure(await responseMessage(response));
  }
  if (response.status === 204) {
    return portfolioSuccess(undefined as T);
  }
  return portfolioSuccess((await response.json()) as T);
}

async function responseMessage(response: Response) {
  const message = await response.text();
  return message || `Portfolio request failed with ${response.status}.`;
}

function portfolioSuccess<T>(data: T): ApiResult<T> {
  return { ok: true, data };
}

function portfolioFailure<T>(message: string): ApiResult<T> {
  return { ok: false, message };
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
