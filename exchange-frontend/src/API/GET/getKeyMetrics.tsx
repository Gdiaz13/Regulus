import axios from "axios";
import type { ICompanyKeyMetrics } from "../../Interfaces/APIResponses/ICompanyKeyMetrics";

// Going to make a single API call for a lot of this get requests, going to make it polymphic so that it can be used for multiple requests 

export const getKeyMetrics = async (symbol: string) => {
    try {
        const data = await axios.get<ICompanyKeyMetrics>(
            `https://financialmodelingprep.com/stable/key-metrics-ttm?symbol=${symbol}&apikey=${import.meta.env.VITE_EXCHANGE_KEY}`
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

