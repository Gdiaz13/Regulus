import { type ICompanyProfile } from "../../Interfaces/APIResponses/ICompanyProfile"
import { requestFmp } from "../fmpClient";
import type { ApiResult } from "../types";

export const getCompanyProfile = async  (query: string): Promise<ApiResult<ICompanyProfile[]>> => {
    return requestFmp<ICompanyProfile[]>("profile", { symbol: query });
}
