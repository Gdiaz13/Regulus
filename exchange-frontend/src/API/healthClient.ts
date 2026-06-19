import type { IApiHealth } from '../Interfaces/APIResponses/IApiHealth';
import { responseMessage } from './responseMessage';
import type { ApiResult } from './types';

export function getApiHealth(): Promise<ApiResult<IApiHealth>> {
  return requestHealth();
}

async function requestHealth(): Promise<ApiResult<IApiHealth>> {
  try {
    const response = await fetch('/api/health');
    return readHealthResponse(response);
  } catch {
    return healthFailure('Unable to connect to the API.');
  }
}

async function readHealthResponse(response: Response): Promise<ApiResult<IApiHealth>> {
  if (!response.ok) {
    return healthFailure(await responseMessage(response, `Health request failed with ${response.status}.`));
  }
  return healthSuccess((await response.json()) as IApiHealth);
}

function healthSuccess(data: IApiHealth): ApiResult<IApiHealth> {
  return { ok: true, data };
}

function healthFailure(message: string): ApiResult<IApiHealth> {
  return { ok: false, message };
}
