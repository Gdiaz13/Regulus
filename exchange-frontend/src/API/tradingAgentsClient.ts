import type { IStockTradingAgentsRequest, IStockTradingAgentsResponse, ITradingAgentsHealth, ITradingAgentsModelInfo } from '../Interfaces/APIResponses/ITradingAgents';
import { apiPath, jsonInit, requestApi } from './apiClient';
import type { ApiResult } from './types';

const base = '/api/trading-agents';

export function analyzeStock(request: IStockTradingAgentsRequest): Promise<ApiResult<IStockTradingAgentsResponse>> {
  return requestApi<IStockTradingAgentsResponse>(apiPath(base, '/stock/analyze'), {
    init: jsonInit({ method: 'POST', body: JSON.stringify(request) }),
    failureMessage: 'TradingAgents stock analysis failed',
    connectionMessage: 'Unable to connect to the TradingAgents gateway.',
  });
}

export function getTradingAgentsHealth(): Promise<ApiResult<ITradingAgentsHealth>> {
  return requestApi<ITradingAgentsHealth>(apiPath(base, '/stock/health'), {
    failureMessage: 'TradingAgents health request failed',
    connectionMessage: 'Unable to connect to the TradingAgents gateway.',
  });
}

export function getTradingAgentsModelInfo(): Promise<ApiResult<ITradingAgentsModelInfo>> {
  return requestApi<ITradingAgentsModelInfo>(apiPath(base, '/stock/model-info'), {
    failureMessage: 'TradingAgents model info request failed',
    connectionMessage: 'Unable to connect to the TradingAgents gateway.',
  });
}
