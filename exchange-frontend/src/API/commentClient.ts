import type { CreateStockComment, IStockComment } from '../Interfaces/APIResponses/IStockComment';
import { apiPath, jsonInit, requestApi } from './apiClient';
import type { ApiResult } from './types';

// Notes belong to a stock, so reads and creates start at /stocks/{id}/comments.
function requestComment<T>(path: string, init?: RequestInit): Promise<ApiResult<T>> {
  return requestApi<T>(apiPath('/api', path), {
    init: jsonInit(init),
    failureMessage: 'Notes request failed',
    connectionMessage: 'Unable to connect to the notes API.',
  });
}

export function getStockComments(stockId: number): Promise<ApiResult<IStockComment[]>> {
  return requestComment<IStockComment[]>(`/stocks/${stockId}/comments`);
}

export function addStockComment(
  stockId: number,
  comment: CreateStockComment,
): Promise<ApiResult<IStockComment>> {
  return requestComment<IStockComment>(`/stocks/${stockId}/comments`, postComment(comment));
}

export function updateStockComment(
  id: number,
  comment: CreateStockComment,
): Promise<ApiResult<IStockComment>> {
  return requestComment<IStockComment>(`/comments/${id}`, putComment(comment));
}

export function deleteStockComment(id: number): Promise<ApiResult<void>> {
  return requestComment<void>(`/comments/${id}`, { method: 'DELETE' });
}

function postComment(comment: CreateStockComment): RequestInit {
  return { method: 'POST', body: JSON.stringify(comment) };
}

function putComment(comment: CreateStockComment): RequestInit {
  return { method: 'PUT', body: JSON.stringify(comment) };
}
