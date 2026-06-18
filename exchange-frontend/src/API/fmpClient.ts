import axios from "axios";
import type { ApiResult } from "./types";

const fmpApiKey = import.meta.env.VITE_EXCHANGE_KEY as string | undefined;
const fmpBaseUrl = "https://financialmodelingprep.com/stable";

type QueryParams = Record<string, string | number | boolean | undefined>;

export async function requestFmp<T>(
  path: string,
  params: QueryParams,
): Promise<ApiResult<T>> {
  if (!fmpApiKey) {
    return apiFailure("Missing Financial Modeling Prep API key.");
  }

  return sendFmpRequest<T>(path, params);
}

async function sendFmpRequest<T>(path: string, params: QueryParams): Promise<ApiResult<T>> {
  try {
    const response = await axios.get<T>(`${fmpBaseUrl}/${path}`, {
      params: withApiKey(params),
    });
    return apiSuccess(response.data);
  } catch (error) {
    return apiFailure(getFmpErrorMessage(error));
  }
}

function withApiKey(params: QueryParams) {
  return { ...params, apikey: fmpApiKey };
}

function getFmpErrorMessage(error: unknown) {
  return axios.isAxiosError(error)
    ? error.response?.data?.message || error.message
    : "Unexpected API error.";
}

function apiSuccess<T>(data: T): ApiResult<T> {
  return { ok: true, data };
}

function apiFailure<T>(message: string): ApiResult<T> {
  return { ok: false, message };
}
