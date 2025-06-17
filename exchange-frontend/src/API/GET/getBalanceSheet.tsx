import { type ICompanyIncomeStatement } from "../../Interfaces/APIResponses/ICompanyIncomeStatement";
import axios from "axios";

export const getIncomeStatement = async (ticker: string) => {
    try {
        const data = await axios.get<ICompanyIncomeStatement>(
            `https://financialmodelingprep.com/stable/income-statement?symbol=${ticker}&apikey=${import.meta.env.VITE_EXCHANGE_KEY}`
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