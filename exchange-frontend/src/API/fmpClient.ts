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
    return {
      ok: false,
      message: "Missing Financial Modeling Prep API key.",
    };
  }

  try {
    const response = await axios.get<T>(`${fmpBaseUrl}/${path}`, {
      params: {
        ...params,
        apikey: fmpApiKey,
      },
    });

    return {
      ok: true,
      data: response.data,
    };
  } catch (error) {
    const message = axios.isAxiosError(error)
      ? error.response?.data?.message || error.message
      : "Unexpected API error.";

    return {
      ok: false,
      message,
    };
  }
}
