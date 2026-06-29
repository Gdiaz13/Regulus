import type { ICompanyBalanceSheet } from "../../Interfaces/APIResponses/ICompanyBalanceSheet";
import { requestMarketData } from "../marketDataClient";
import type { ApiResult } from "../types";


export const getBalanceSheet = async (ticker: string): Promise<ApiResult<ICompanyBalanceSheet[]>> => {
    return requestMarketData<ICompanyBalanceSheet[]>("balance-sheet-statement", { symbol: ticker });
};
