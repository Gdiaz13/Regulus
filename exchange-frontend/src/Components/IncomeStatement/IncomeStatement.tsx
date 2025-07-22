import { useEffect, useState } from 'react'
import { incomeStatementConfig } from './Config/IncomeStatementConfig'
import { useOutletContext } from 'react-router'
import type { ICompanyIncomeStatement } from '../../Interfaces/APIResponses/ICompanyIncomeStatement';
import { getIncomeStatement } from '../../API/GET/getIncomeStatement';
import Table from '../Table/Table';
import Spinner from '../Spinner/Spinner';


const IncomeStatement = () => {
  const ticker = useOutletContext<string>();
  const [incomeStatement, setIncomeStatement] = useState<ICompanyIncomeStatement[]>();

  useEffect(() => {
    const getIncomeStatementData = async () => {
      const result = await getIncomeStatement(ticker);
      setIncomeStatement(result!.data);
    };
    getIncomeStatementData();
  }, []);

  return (
    <>
    { incomeStatement ? 
    <>
    <Table config={incomeStatementConfig} data={incomeStatement} />
    </> : <Spinner /> }
    </>
  )
};

export default IncomeStatement