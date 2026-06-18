import type { ICompanyBalanceSheet } from "../../Interfaces/APIResponses/ICompanyBalanceSheet";
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";


export const getBalanceSheet = async (ticker: string): Promise<ApiResult<ICompanyBalanceSheet[]>> => {
    return requestFmp<ICompanyBalanceSheet[]>("balance-sheet-statement", { symbol: ticker });
};
