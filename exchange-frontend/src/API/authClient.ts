import type { IAuthResponse, ICurrentUser, LoginRequest, RegisterRequest } from '../Interfaces/APIResponses/IAuth';
import { apiPath, jsonInit, requestApi } from './apiClient';
import type { ApiResult } from './types';

function requestAuth<T>(path: string, init?: RequestInit): Promise<ApiResult<T>> {
  return requestApi<T>(apiPath('/api/v1/auth', path), {
    init: jsonInit(init),
    failureMessage: 'Authentication request failed',
    connectionMessage: 'Unable to connect to the auth API.',
  });
}

export function loginUser(request: LoginRequest): Promise<ApiResult<IAuthResponse>> {
  return requestAuth<IAuthResponse>('/login', postAuth(request));
}

export function registerUser(request: RegisterRequest): Promise<ApiResult<IAuthResponse>> {
  return requestAuth<IAuthResponse>('/register', postAuth(request));
}

export function getCurrentUser(): Promise<ApiResult<ICurrentUser>> {
  return requestAuth<ICurrentUser>('/me');
}

export function logoutUser(): Promise<ApiResult<void>> {
  return requestAuth<void>('/logout', { method: 'POST' });
}

function postAuth(request: LoginRequest | RegisterRequest): RequestInit {
  return { method: 'POST', body: JSON.stringify(request) };
}
