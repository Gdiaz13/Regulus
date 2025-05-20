import axios from 'axios';
// Update the import path below to the correct relative path where ICompanySearch is defined.
// For example, if the file is in src/Interfaces/ICompanySearch.ts, use:
// Update the path below to the correct relative path if needed
import type {ICompanySearch} from '../Interfaces/ICompanySearch';


interface SearchResponse {
    data: ICompanySearch[];
}
export const searchCompanies = async (query: string) => {
    try {
        const data = await axios.get<SearchResponse>(
            `https://financialmodelingprep.com/stable/search-symbol?query=${query}&apikey=${process.env.REACT_APP_API_KEY}`
        )
    }
}