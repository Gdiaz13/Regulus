import type {ICompanySearch} from '../../Interfaces/APIResponses/ICompanySearch';
import { requestFmp } from '../fmpClient';
import type { ApiResult } from '../types';

const getCompanies = async (query: string): Promise<ApiResult<ICompanySearch[]>> => {
    return requestFmp<ICompanySearch[]>('search-name', { query });
};

export default getCompanies;
