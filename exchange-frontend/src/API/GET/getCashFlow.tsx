import type { ICompanyCashFlow } from "../../Interfaces/APIResponses/ICompanyCashFlow";
import { requestMarketData } from "../marketDataClient";
import type { ApiResult } from "../types";

export const getCashFlow = async (ticker: string): Promise<ApiResult<ICompanyCashFlow[]>> => {
    return requestMarketData<ICompanyCashFlow[]>("cash-flow-statement-growth", { symbol: ticker });
};
