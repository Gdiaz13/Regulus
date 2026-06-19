import type { CreateStockComment, IStockComment } from '../Interfaces/APIResponses/IStockComment';
import { responseMessage } from './responseMessage';
import type { ApiResult } from './types';

async function requestComment<T>(path: string, init?: RequestInit): Promise<ApiResult<T>> {
  try {
    const response = await fetch(`/api${path}`, commentInit(init));
    return readCommentResponse(response);
  } catch {
    return commentFailure('Unable to connect to the notes API.');
  }
}

function commentInit(init?: RequestInit): RequestInit {
  return { ...init, headers: commentHeaders(init) };
}

function commentHeaders(init?: RequestInit) {
  const headers = new Headers(init?.headers);
  headers.set('Content-Type', 'application/json');
  return headers;
}

async function readCommentResponse<T>(response: Response): Promise<ApiResult<T>> {
  if (!response.ok) {
    return commentFailure(await responseMessage(response, `Notes request failed with ${response.status}.`));
  }
  if (response.status === 204) {
    return commentSuccess(undefined as T);
  }
  return commentSuccess((await response.json()) as T);
}

function commentSuccess<T>(data: T): ApiResult<T> {
  return { ok: true, data };
}

function commentFailure<T>(message: string): ApiResult<T> {
  return { ok: false, message };
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

export function deleteStockComment(id: number): Promise<ApiResult<void>> {
  return requestComment<void>(`/comments/${id}`, { method: 'DELETE' });
}

function postComment(comment: CreateStockComment): RequestInit {
  return { method: 'POST', body: JSON.stringify(comment) };
}
