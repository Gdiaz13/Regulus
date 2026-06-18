import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { useParams } from 'react-router-dom';
import type { LoadStatus } from '../../API/types';
import { getCompanyProfile } from '../../API/GET/getCompanyProfile';
import Dashboard from '../../Components/Dashboard/Dashboard';
import DashboardCard from '../../Components/Dashboard/DashboardCard';
import Sidebar from '../../Components/Sidebar/Sidebar';
import Spinner from '../../Components/Spinner/Spinner';
import type { ICompanyProfile } from '../../Interfaces/APIResponses/ICompanyProfile';
import { formatCurrency } from '../../lib/formatters';

const messageStyle = {
  color: '#FFD700',
  marginTop: '6rem',
  textAlign: 'center',
} satisfies CSSProperties;

const CompanyPage = () => {
  const { ticker = '' } = useParams();
  const [company, setCompany] = useState<ICompanyProfile | null>(null);
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const getProfileInfo = async () => {
      if (!ticker) {
        setStatus('error');
        setMessage('Missing company ticker.');
        return;
      }

      setStatus('loading');
      setMessage(null);

      const result = await getCompanyProfile(ticker);

      if (result.ok && result.data.length > 0) {
        setCompany(result.data[0]);
        setStatus('success');
      } else if (result.ok) {
        setCompany(null);
        setStatus('empty');
        setMessage(`No company profile found for ${ticker.toUpperCase()}.`);
      } else {
        setCompany(null);
        setStatus('error');
        setMessage(result.message);
      }
    };

    getProfileInfo();
  }, [ticker]);

  if (status === 'loading') {
    return <Spinner />;
  }

  if (!company) {
    return <div style={messageStyle}>{message}</div>;
  }

  return (
    <div>
      <Sidebar />
      <Dashboard ticker={ticker}>
        <DashboardCard title="Company Name" value={company.companyName} />
        <DashboardCard title="Company Symbol" value={company.symbol} />
        <DashboardCard title="Price" value={formatCurrency(company.price)} />
        <DashboardCard title="Market Cap" value={formatCurrency(company.marketCap)} />
        <DashboardCard title="Change Percentage" value={`${company.changePercentage}%`} />
        <DashboardCard title="Industry" value={company.industry} />
        <DashboardCard title="CEO" value={company.ceo} />
      </Dashboard>
    </div>
  );
};

export default CompanyPage;
