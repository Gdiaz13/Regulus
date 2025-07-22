import  { useEffect, useState } from 'react'
import { config } from './Config/CashFlowConfig';
import { useOutletContext } from 'react-router-dom';
import type { ICompanyCashFlow } from '../../Interfaces/APIResponses/ICompanyCashFlow';
import { getCashFlow } from '../../API/GET/getCashFlow';
import Table from '../Table/Table';
import Spinner from '../Spinner/Spinner';

type Props = {}

const CashFlowStatement = (props: Props) => {
    const ticker = useOutletContext<string>();
    const [cashflowData, setCashflowData] = useState<ICompanyCashFlow[]>();
    useEffect(() => {
        const fetchCashFlow = async () => {
        const result = await getCashFlow(ticker!);
        setCashflowData(result!.data);
    };
        fetchCashFlow();
    }, []);
  return <>
  { cashflowData ? (
    <Table config={config} data={cashflowData} />
  ) : ( 
    <Spinner />
  )}
  </>;
};

export default CashFlowStatement;