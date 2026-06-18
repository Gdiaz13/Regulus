import type { ICompanyCashFlow } from "../../Interfaces/APIResponses/ICompanyCashFlow";
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";

export const getCashFlow = async (ticker: string): Promise<ApiResult<ICompanyCashFlow[]>> => {
    return requestFmp<ICompanyCashFlow[]>("cash-flow-statement-growth", { symbol: ticker });
};
