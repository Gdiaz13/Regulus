import type {ICompanySearch} from '../../Interfaces/APIResponses/ICompanySearch';
import { requestMarketData } from '../marketDataClient';
import type { ApiResult } from '../types';

const getCompanies = async (query: string): Promise<ApiResult<ICompanySearch[]>> => {
    return requestMarketData<ICompanySearch[]>('search-name', { query });
};

export default getCompanies;
