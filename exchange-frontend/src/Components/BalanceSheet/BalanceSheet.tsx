import { useState, useEffect } from 'react'
import { useOutletContext } from 'react-router-dom'
import type { ICompanyBalanceSheet } from '../../Interfaces/APIResponses/ICompanyBalanceSheet';
import { getBalanceSheet } from '../../API/GET/getBalanceSheet';
import { balanceSheetConfig } from './Config/balanceSheetConfig';
import RatioList from '../RatioList/RatioList';



const BalanceSheet = () => {
  const ticker = useOutletContext<string>();
  const [balanceSheet, setBalanceSheet] = useState<ICompanyBalanceSheet | any>();

  useEffect(() => {
    const getData = async () => {
      const value = await getBalanceSheet(ticker!);
      if (Array.isArray(value) && value.length > 0) {
        setBalanceSheet(value[0]);
        console.log('Balance Sheet Data:', value[0]);
      } else {
        setBalanceSheet(null);
        if (value && typeof value === 'object' && 'error' in value) {
          console.error('Balance Sheet Error:', value.error);
        }
      }
    };
    getData();
  }, [])


  return <>
   {balanceSheet ? (
     <RatioList config={balanceSheetConfig} data={balanceSheet} />
    ) : (
      <h1> Company Balance Sheet Not Found</h1>
   )}
  </>
}

export default BalanceSheet