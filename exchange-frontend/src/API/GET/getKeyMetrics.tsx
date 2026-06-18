import type { ICompanyKeyMetrics } from "../../Interfaces/APIResponses/ICompanyKeyMetrics";
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";

// Going to make a single API call for a lot of this get requests, going to make it polymphic so that it can be used for multiple requests 

export const getKeyMetrics = async (symbol: string): Promise<ApiResult<ICompanyKeyMetrics[]>> => {
    return requestFmp<ICompanyKeyMetrics[]>("key-metrics-ttm", { symbol });
};

