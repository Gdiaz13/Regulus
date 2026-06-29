import type { ICompanyKeyMetrics } from "../../Interfaces/APIResponses/ICompanyKeyMetrics";
import { requestMarketData } from "../marketDataClient";
import type { ApiResult } from "../types";

export const getKeyMetrics = async (symbol: string): Promise<ApiResult<ICompanyKeyMetrics[]>> => {
    return requestMarketData<ICompanyKeyMetrics[]>("key-metrics-ttm", { symbol });
};

