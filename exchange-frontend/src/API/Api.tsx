import axios from 'axios';
import type {ICompanySearch} from '../Interfaces/ICompanySearch';

const apiKey = import.meta.env.VITE_EXCHANGE_KEY;

interface SearchResponse {
    data: ICompanySearch[];
}
export const searchCompanies = async (query: string) => {
    try {
        const data = await axios.get<SearchResponse>(
            `https://financialmodelingprep.com/stable/search-symbol?query=${query}&apikey=${apiKey}`
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