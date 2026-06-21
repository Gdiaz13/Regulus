import { responseMessage } from './responseMessage';
import type { ApiResult } from './types';

type QueryValue = string | number | boolean | undefined;

export type QueryParams = Record<string, QueryValue>;

type ApiRequestOptions = {
  init?: RequestInit;
  failureMessage: string;
  connectionMessage: string;
};

// All browser calls use this helper so every API returns the same ApiResult shape.
export async function requestApi<T>(url: string, options: ApiRequestOptions): Promise<ApiResult<T>> {
  const response = await fetchResponse(url, options);
  return response.ok ? readApiResponse(response.data, options.failureMessage) : response;
}

async function fetchResponse(url: string, options: ApiRequestOptions): Promise<ApiResult<Response>> {
  try {
    return apiSuccess(await fetch(url, options.init));
  } catch {
    return apiFailure(options.connectionMessage);
  }
}

export function apiPath(basePath: string, path = '', params?: QueryParams) {
  const fullPath = `${basePath}${path}`;
  return params ? withQuery(fullPath, params) : fullPath;
}

export function apiPathWithQuery(path: string, params: QueryParams) {
  return withQuery(path, params);
}

export function jsonInit(init?: RequestInit): RequestInit {
  if (!hasBody(init)) {
    return init ?? {};
  }
  return { ...init, headers: jsonHeaders(init) };
}

function hasBody(init?: RequestInit) {
  return init?.body !== undefined && init.body !== null;
}

async function readApiResponse<T>(response: Response, fallback: string): Promise<ApiResult<T>> {
  if (!response.ok) {
    return apiFailure(await responseMessage(response, statusMessage(fallback, response.status)));
  }
  if (response.status === 204) {
    return apiSuccess(undefined as T);
  }
  return parseJsonResponse(response, fallback);
}

async function parseJsonResponse<T>(response: Response, fallback: string): Promise<ApiResult<T>> {
  try {
    return apiSuccess((await response.json()) as T);
  } catch {
    return apiFailure(statusMessage(fallback, response.status));
  }
}

function withQuery(path: string, params: QueryParams) {
  const query = new URLSearchParams(cleanParams(params)).toString();
  return query ? `${path}?${query}` : path;
}

function cleanParams(params: QueryParams): Record<string, string> {
  return Object.fromEntries(validEntries(params));
}

function validEntries(params: QueryParams) {
  return Object.entries(params).filter(hasQueryValue).map(stringEntry);
}

function hasQueryValue(entry: [string, QueryValue]) {
  return entry[1] !== undefined;
}

function stringEntry([key, value]: [string, QueryValue]) {
  return [key, String(value)];
}

function jsonHeaders(init?: RequestInit) {
  const headers = new Headers(init?.headers);
  headers.set('Content-Type', 'application/json');
  return headers;
}

function statusMessage(message: string, status: number) {
  return `${message} with ${status}.`;
}

function apiSuccess<T>(data: T): ApiResult<T> {
  return { ok: true, data };
}

function apiFailure<T>(message: string): ApiResult<T> {
  return { ok: false, message };
}
