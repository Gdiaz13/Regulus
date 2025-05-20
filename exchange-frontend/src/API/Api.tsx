import axios from 'axios';
import type {ICompanySearch} from '../Interfaces/ICompanySearch';


interface SearchResponse {
    data: ICompanySearch[];
}
export const searchCompanies = async (query: string) => {
    try {
        const data = await axios.get<SearchResponse>(
            `https://financialmodelingprep.com/stable/search-symbol?query=${query}&apikey=${import.meta.env.ExchangeKey}`
        );
        return data;
    } 
    
    catch (error) {
        if ((error as any).isAxiosError) {
            console.log('Error message: ', (error as Error).message);
            return (error as Error).message;
        } else {
            console.log('Unexpected error: ', (error as Error).message);
            return (error as Error).message;
        }
    }
};