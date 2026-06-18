import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useOutletContext } from 'react-router-dom';
import type { LoadStatus } from '../../API/types';
import { getKeyMetrics } from '../../API/GET/getKeyMetrics';
import type { ICompanyKeyMetrics } from '../../Interfaces/APIResponses/ICompanyKeyMetrics';
import RatioList from '../RatioList/RatioList';
import Spinner from '../Spinner/Spinner';
import { tableConfig } from './Config/CompanyProfileConfig';

const messageStyle = {
  color: '#FFD700',
  marginTop: '2rem',
  textAlign: 'center',
} satisfies CSSProperties;

const CompanyProfile = () => {
  const ticker = useOutletContext<string>();
  const [companyData, setCompanyData] = useState<ICompanyKeyMetrics | null>(null);
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const getCompanyKeyMetrics = async () => {
      setStatus('loading');
      setMessage(null);

      const value = await getKeyMetrics(ticker);

      if (value.ok && value.data.length > 0) {
        setCompanyData(value.data[0]);
        setStatus('success');
      } else if (value.ok) {
        setCompanyData(null);
        setStatus('empty');
        setMessage('No key metrics found for this ticker.');
      } else {
        setCompanyData(null);
        setStatus('error');
        setMessage(value.message);
      }
    };

    getCompanyKeyMetrics();
  }, [ticker]);

  if (status === 'loading') {
    return <Spinner />;
  }

  if (!companyData) {
    return <div style={messageStyle}>{message}</div>;
  }

  return <RatioList data={companyData} config={tableConfig} />;
};

export default CompanyProfile;
