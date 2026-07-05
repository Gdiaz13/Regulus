import type {
  IAiOverview,
  IModelAccuracySummary,
  IPredictAsset,
  IPredictionAccuracyResponse,
  IPredictionHealth,
  IPredictionHistoryItem,
} from '../Interfaces/APIResponses/IPrediction';
import { apiPath, jsonInit, requestApi } from './apiClient';
import type { ApiResult } from './types';

// Prediction screens use this file instead of building /api/predict URLs by hand.
// The browser only ever talks to the C# gateway; the gateway calls the AI services.
export function postPrediction(assets: IPredictAsset[]): Promise<ApiResult<IAiOverview>> {
  return requestApi<IAiOverview>(apiPath('/api/predict'), {
    init: jsonInit({ method: 'POST', body: JSON.stringify({ assets }) }),
    failureMessage: 'Prediction request failed',
    connectionMessage: 'Unable to connect to the prediction API.',
  });
}

export function getPredictionHealth(): Promise<ApiResult<IPredictionHealth>> {
  return requestApi<IPredictionHealth>(apiPath('/api/predict', '/health'), {
    failureMessage: 'Prediction health request failed',
    connectionMessage: 'Unable to connect to the prediction API.',
  });
}

export function getPredictionHistory(take = 10): Promise<ApiResult<IPredictionHistoryItem[]>> {
  return requestApi<IPredictionHistoryItem[]>(apiPath('/api/predict', '/history', { take }), {
    failureMessage: 'Prediction history request failed',
    connectionMessage: 'Unable to connect to the prediction history API.',
  });
}

export function getPredictionAccuracy(take = 25): Promise<ApiResult<IPredictionAccuracyResponse[]>> {
  return requestApi<IPredictionAccuracyResponse[]>(apiPath('/api/predict', '/accuracy', { take }), {
    failureMessage: 'Prediction accuracy request failed',
    connectionMessage: 'Unable to connect to the prediction accuracy API.',
  });
}

export function getPredictionAccuracySummary(take = 25): Promise<ApiResult<IModelAccuracySummary[]>> {
  return requestApi<IModelAccuracySummary[]>(apiPath('/api/predict', '/accuracy/summary', { take }), {
    failureMessage: 'Prediction accuracy summary request failed',
    connectionMessage: 'Unable to connect to the prediction accuracy summary API.',
  });
}
