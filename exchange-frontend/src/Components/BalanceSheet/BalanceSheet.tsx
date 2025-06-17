import React, { useEffect } from 'react'
import { useOutletContext } from 'react-router-dom'
import type { ICompanyBalanceSheet } from '../../Interfaces/APIResponses/ICompanyBalanceSheet';
import { getBalanceSheet } from '../../API/GET/getBalanceSheet';
import { balanceSheetConfig } from './Config/balanceSheetConfig';
import RatioList from '../RatioList/RatioList';

type Props = {}

const BalanceSheet = (props: Props) => {
  const ticker = useOutletContext<string>();
  const [balanceSheet, setBalanceSheet] = React.useState<ICompanyBalanceSheet>();

  useEffect(() => {
    const getData = async () => {
        const value = await getBalanceSheet(ticker!);
        setBalanceSheet(value?.data[0]);
        console.log('Balance Sheet Data:', value?.data[0]);
    };
        getData();
  }, [])


  return <>
   {balanceSheet ? (
     <RatioList config={balanceSheetConfig} data={balanceSheet} />
    ) : (
      <h1> Company Balance Sheet Not Found</h1>
   )};
  </>
}

export default BalanceSheet