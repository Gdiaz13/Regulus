import { apiPathWithQuery, requestApi } from "./apiClient";
import type { QueryParams } from "./apiClient";
import type { ApiResult } from "./types";

// The browser talks only to Regulas.Api; the API adds provider keys server-side.
export async function requestMarketData<T>(
  path: string,
  params: QueryParams,
): Promise<ApiResult<T>> {
  return requestApi<T>(apiPathWithQuery(`/api/market-data/${path}`, params), {
    failureMessage: 'Market data request failed',
    connectionMessage: 'Unable to connect to the market data API.',
  });
}
