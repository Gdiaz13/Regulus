import React, { useEffect, useState } from 'react'
import { incomeStatementConfig } from './Config/IncomeStatementConfig'
import { useOutletContext } from 'react-router'
import type { ICompanyIncomeStatement } from '../../Interfaces/APIResponses/ICompanyIncomeStatement';
import { getIncomeStatement } from '../../API/GET/getIncomeStatement';
import Table from '../Table/Table';

interface Props  {}

const IncomeStatement = (props: Props) => {
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
    </> : <>Loading...</> }
    </>
  )
};

export default IncomeStatement