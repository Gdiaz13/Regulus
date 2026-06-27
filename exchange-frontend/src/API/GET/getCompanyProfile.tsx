import { type ICompanyProfile } from "../../Interfaces/APIResponses/ICompanyProfile"
import { requestMarketData } from "../marketDataClient";
import type { ApiResult } from "../types";

export const getCompanyProfile = async  (query: string): Promise<ApiResult<ICompanyProfile[]>> => {
    return requestMarketData<ICompanyProfile[]>("profile", { symbol: query });
}
