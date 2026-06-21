import type { ICompanyKeyMetrics } from "../../Interfaces/APIResponses/ICompanyKeyMetrics";
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";

export const getKeyMetrics = async (symbol: string): Promise<ApiResult<ICompanyKeyMetrics[]>> => {
    return requestFmp<ICompanyKeyMetrics[]>("key-metrics-ttm", { symbol });
};

