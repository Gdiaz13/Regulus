import { type ICompanyProfile } from "../../Interfaces/APIResponses/ICompanyProfile"
import axios from "axios"

export const getCompanyProfile = async  (query: string) => {
    try {
        const data = await axios.get<ICompanyProfile[]>(
            `https://financialmodelingprep.com/stable/profile?symbol=${query}&apikey=${import.meta.env.VITE_EXCHANGE_KEY}`
        )
        return data
    } catch (error: any) {
        if (error.isAxiosError) {
            console.log('Error message: ', error.message)
            return error.message
        } else {
            console.log('Unexpected error: ', error.message)
            return error.message
        }
    }
}