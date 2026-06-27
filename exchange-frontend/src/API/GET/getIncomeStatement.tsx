import { type ICompanyIncomeStatement } from "../../Interfaces/APIResponses/ICompanyIncomeStatement";
import { requestMarketData } from "../marketDataClient";
import type { ApiResult } from "../types";

export const getIncomeStatement = async (ticker: string): Promise<ApiResult<ICompanyIncomeStatement[]>> => {
    return requestMarketData<ICompanyIncomeStatement[]>("income-statement", { symbol: ticker });
};
