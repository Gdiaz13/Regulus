import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useOutletContext } from 'react-router-dom';
import type { LoadStatus } from '../../API/types';
import { getBalanceSheet } from '../../API/GET/getBalanceSheet';
import type { ICompanyBalanceSheet } from '../../Interfaces/APIResponses/ICompanyBalanceSheet';
import RatioList from '../RatioList/RatioList';
import Spinner from '../Spinner/Spinner';
import { balanceSheetConfig } from './Config/balanceSheetConfig';

const messageStyle = {
  color: '#FFD700',
  marginTop: '2rem',
  textAlign: 'center',
} satisfies CSSProperties;

const BalanceSheet = () => {
  const ticker = useOutletContext<string>();
  const [balanceSheet, setBalanceSheet] = useState<ICompanyBalanceSheet | null>(null);
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const getData = async () => {
      setStatus('loading');
      setMessage(null);

      const value = await getBalanceSheet(ticker);

      if (value.ok && value.data.length > 0) {
        setBalanceSheet(value.data[0]);
        setStatus('success');
      } else if (value.ok) {
        setBalanceSheet(null);
        setStatus('empty');
        setMessage('No balance sheet found for this ticker.');
      } else {
        setBalanceSheet(null);
        setStatus('error');
        setMessage(value.message);
      }
    };

    getData();
  }, [ticker]);

  if (status === 'loading') {
    return <Spinner />;
  }

  if (!balanceSheet) {
    return <div style={messageStyle}>{message}</div>;
  }

  return <RatioList config={balanceSheetConfig} data={balanceSheet} />;
};

export default BalanceSheet;
