import type { ICompanyBalanceSheet } from "../../Interfaces/APIResponses/ICompanyBalanceSheet";
import axios from "axios";


export const getBalanceSheet = async (ticker: string) => {
    try {
        const response = await axios.get<ICompanyBalanceSheet[]>(
            `https://financialmodelingprep.com/stable/balance-sheet-statement?symbol=${ticker}&apikey=${import.meta.env.VITE_EXCHANGE_KEY}`
        );
        return response.data;
      
    } catch (error: any) {
        if (error.isAxiosError) {
            console.error('Balance sheet API error:', error.message);
            return { error: 'Balance sheet API error: ' + error.message };
        } else {
            console.error('Unexpected balance sheet error:', error.message);
            return { error: 'Unexpected balance sheet error: ' + error.message };
        }
    }
};