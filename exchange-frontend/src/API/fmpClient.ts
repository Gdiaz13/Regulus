import { apiPathWithQuery, requestApi } from "./apiClient";
import type { QueryParams } from "./apiClient";
import type { ApiResult } from "./types";

// The browser calls our API proxy; the .NET API adds the real FMP key.
export async function requestFmp<T>(
  path: string,
  params: QueryParams,
): Promise<ApiResult<T>> {
  return requestApi<T>(apiPathWithQuery(`/api/market-data/${path}`, params), {
    failureMessage: 'Market data request failed',
    connectionMessage: 'Unable to connect to the market data API.',
  });
}
