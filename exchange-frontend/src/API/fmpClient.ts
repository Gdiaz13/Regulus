import type { ApiResult } from "./types";

type QueryParams = Record<string, string | number | boolean | undefined>;

// The browser calls our API proxy; the .NET API adds the real FMP key.
export async function requestFmp<T>(
  path: string,
  params: QueryParams,
): Promise<ApiResult<T>> {
  return requestMarketData<T>(path, params);
}

async function requestMarketData<T>(path: string, params: QueryParams): Promise<ApiResult<T>> {
  try {
    const response = await fetch(marketDataUrl(path, params));
    return readMarketData<T>(response);
  } catch {
    return apiFailure("Unable to connect to the market data API.");
  }
}

async function readMarketData<T>(response: Response): Promise<ApiResult<T>> {
  if (!response.ok) {
    return apiFailure(await responseMessage(response));
  }
  return apiSuccess((await response.json()) as T);
}

function marketDataUrl(path: string, params: QueryParams) {
  const query = new URLSearchParams(cleanParams(params));
  return `/api/market-data/${path}?${query.toString()}`;
}

function cleanParams(params: QueryParams): Record<string, string> {
  return Object.fromEntries(validEntries(params));
}

function validEntries(params: QueryParams) {
  return Object.entries(params)
    .filter(([, value]) => value !== undefined)
    .map(([key, value]) => [key, String(value)]);
}

async function responseMessage(response: Response) {
  const message = await response.text();
  return parseErrorMessage(message) || `Market data request failed with ${response.status}.`;
}

function parseErrorMessage(message: string) {
  if (!message) {
    return null;
  }
  return readJsonMessage(message) ?? message;
}

function readJsonMessage(message: string) {
  try {
    const parsed = JSON.parse(message) as ErrorPayload;
    return parsed.message ?? parsed.detail ?? null;
  } catch {
    return null;
  }
}

function apiSuccess<T>(data: T): ApiResult<T> {
  return { ok: true, data };
}

function apiFailure<T>(message: string): ApiResult<T> {
  return { ok: false, message };
}

type ErrorPayload = {
  message?: string;
  detail?: string;
};
