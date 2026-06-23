import type { ICaptureResult, IPriceHistory } from '../Interfaces/APIResponses/IPriceHistory';
import { apiPath, jsonInit, requestApi } from './apiClient';
import type { ApiResult } from './types';

// Price-history screens use this file instead of building /api/price-history
// URLs by hand. The browser only ever talks to the C# gateway, which is the
// only thing that holds the market-data key and calls the provider.
const base = '/api/price-history';

export function getPriceHistory(symbol: string, assetType: string): Promise<ApiResult<IPriceHistory>> {
  return requestApi<IPriceHistory>(apiPath(base, `/${encodeURIComponent(symbol)}`, { assetType }), {
    failureMessage: 'Price history request failed',
    connectionMessage: 'Unable to connect to the price history API.',
  });
}

export function capturePriceHistory(symbol: string, assetType: string): Promise<ApiResult<ICaptureResult>> {
  return requestApi<ICaptureResult>(apiPath(base, `/${encodeURIComponent(symbol)}/capture`, { assetType }), {
    init: jsonInit({ method: 'POST' }),
    failureMessage: 'Price history capture failed',
    connectionMessage: 'Unable to connect to the price history API.',
  });
}
