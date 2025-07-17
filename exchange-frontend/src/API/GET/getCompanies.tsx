import axios from 'axios';
import type {ICompanySearch} from '../../Interfaces/APIResponses/ICompanySearch';

const apiKey = import.meta.env.VITE_EXCHANGE_KEY;
// not a big fan of the stock APIS i am finding might switch to a crypto API later
interface SearchResponse {
    data: ICompanySearch[];
}
const getCompanies = async (query: string) => {
    try {
        const data = await axios.get<SearchResponse>(
            `https://financialmodelingprep.com/stable/search-name?query=${query}&apikey=${apiKey}`
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

export default getCompanies;