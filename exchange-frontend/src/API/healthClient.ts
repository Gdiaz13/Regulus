import type { IApiHealth } from '../Interfaces/APIResponses/IApiHealth';
import { requestApi } from './apiClient';
import type { ApiResult } from './types';

export function getApiHealth(): Promise<ApiResult<IApiHealth>> {
  return requestApi<IApiHealth>('/api/health', {
    failureMessage: 'Health request failed',
    connectionMessage: 'Unable to connect to the API.',
  });
}
