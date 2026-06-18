import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useOutletContext } from 'react-router';
import type { LoadStatus } from '../../API/types';
import { getIncomeStatement } from '../../API/GET/getIncomeStatement';
import type { ICompanyIncomeStatement } from '../../Interfaces/APIResponses/ICompanyIncomeStatement';
import Spinner from '../Spinner/Spinner';
import Table from '../Table/Table';
import { incomeStatementConfig } from './Config/IncomeStatementConfig';

const messageStyle = {
  color: '#FFD700',
  marginTop: '2rem',
  textAlign: 'center',
} satisfies CSSProperties;

const IncomeStatement = () => {
  const ticker = useOutletContext<string>();
  const [incomeStatement, setIncomeStatement] = useState<ICompanyIncomeStatement[]>([]);
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const getIncomeStatementData = async () => {
      setStatus('loading');
      setMessage(null);

      const result = await getIncomeStatement(ticker);

      if (result.ok && result.data.length > 0) {
        setIncomeStatement(result.data);
        setStatus('success');
      } else if (result.ok) {
        setIncomeStatement([]);
        setStatus('empty');
        setMessage('No income statements found for this ticker.');
      } else {
        setIncomeStatement([]);
        setStatus('error');
        setMessage(result.message);
      }
    };

    getIncomeStatementData();
  }, [ticker]);

  if (status === 'loading') {
    return <Spinner />;
  }

  if (incomeStatement.length === 0) {
    return <div style={messageStyle}>{message}</div>;
  }

  return <Table config={incomeStatementConfig} data={incomeStatement} />;
};

export default IncomeStatement;
