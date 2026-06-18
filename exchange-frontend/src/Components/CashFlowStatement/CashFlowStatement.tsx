import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useOutletContext } from 'react-router-dom';
import type { LoadStatus } from '../../API/types';
import { getCashFlow } from '../../API/GET/getCashFlow';
import type { ICompanyCashFlow } from '../../Interfaces/APIResponses/ICompanyCashFlow';
import Spinner from '../Spinner/Spinner';
import Table from '../Table/Table';
import { config } from './Config/CashFlowConfig';

const messageStyle = {
  color: '#FFD700',
  marginTop: '2rem',
  textAlign: 'center',
} satisfies CSSProperties;

const CashFlowStatement = () => {
  const ticker = useOutletContext<string>();
  const [cashflowData, setCashflowData] = useState<ICompanyCashFlow[]>([]);
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const fetchCashFlow = async () => {
      setStatus('loading');
      setMessage(null);

      const result = await getCashFlow(ticker);

      if (result.ok && result.data.length > 0) {
        setCashflowData(result.data);
        setStatus('success');
      } else if (result.ok) {
        setCashflowData([]);
        setStatus('empty');
        setMessage('No cash flow statements found for this ticker.');
      } else {
        setCashflowData([]);
        setStatus('error');
        setMessage(result.message);
      }
    };

    fetchCashFlow();
  }, [ticker]);

  if (status === 'loading') {
    return <Spinner />;
  }

  if (cashflowData.length === 0) {
    return <div style={messageStyle}>{message}</div>;
  }

  return <Table config={config} data={cashflowData} />;
};

export default CashFlowStatement;
