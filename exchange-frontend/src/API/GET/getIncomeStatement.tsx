import { type ICompanyIncomeStatement } from "../../Interfaces/APIResponses/ICompanyIncomeStatement";
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";

export const getIncomeStatement = async (ticker: string): Promise<ApiResult<ICompanyIncomeStatement[]>> => {
    return requestFmp<ICompanyIncomeStatement[]>("income-statement", { symbol: ticker });
};
