import type { ICompanyCashFlow } from "../../Interfaces/APIResponses/ICompanyCashFlow";
import axios from "axios";

export const getCashFlow = async (ticker: string) => {
    try {
        const data = await axios.get<ICompanyCashFlow[]>(
            `https://financialmodelingprep.com/stable/cash-flow-statement-growth?symbol=${ticker}&apikey=${import.meta.env.VITE_EXCHANGE_KEY}`
        );
        return data;
    } catch (error: any) {
        if (error.isAxiosError) {
            console.log('Error message: ', error.message);
            return error.message;
        } else {
            console.log('Unexpected error: ', error.message); 
            return error.message;
        }
    }
};