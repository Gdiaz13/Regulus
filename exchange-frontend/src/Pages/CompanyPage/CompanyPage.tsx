import type { CSSProperties } from 'react';
import { useParams } from 'react-router-dom';
import { getCompanyProfile } from '../../API/GET/getCompanyProfile';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import Dashboard from '../../Components/Dashboard/Dashboard';
import DashboardCard from '../../Components/Dashboard/DashboardCard';
import Sidebar from '../../Components/Sidebar/Sidebar';
import type { ICompanyProfile } from '../../Interfaces/APIResponses/ICompanyProfile';
import { useTickerFirstResource } from '../../hooks/useTickerResource';
import { formatCurrency } from '../../lib/formatters';

const messageStyle = {
  color: '#FFD700',
  marginTop: '6rem',
  textAlign: 'center',
} satisfies CSSProperties;

type DashboardCardValue = {
  title: string;
  value: string | number;
};

const CompanyPage = () => {
  const { ticker = '' } = useParams();
  const emptyMessage = profileEmptyMessage(ticker);
  const state = useTickerFirstResource(ticker, getCompanyProfile, emptyMessage);

  if (!state.data) {
    return <ResourceStatus status={state.status} message={state.message} style={messageStyle} />;
  }

  return <CompanyDashboard ticker={ticker} company={state.data} />;
};

function CompanyDashboard(props: { ticker: string; company: ICompanyProfile }) {
  return (
    <div>
      <Sidebar />
      <Dashboard ticker={props.ticker}>{renderDashboardCards(props.company)}</Dashboard>
    </div>
  );
}

function renderDashboardCards(company: ICompanyProfile) {
  return companyCards(company).map((card) => (
    <DashboardCard key={card.title} title={card.title} value={card.value} />
  ));
}

function companyCards(company: ICompanyProfile): DashboardCardValue[] {
  return [
    { title: 'Company Name', value: company.companyName },
    { title: 'Company Symbol', value: company.symbol },
    { title: 'Price', value: formatCurrency(company.price) },
    { title: 'Market Cap', value: formatCurrency(company.marketCap) },
    { title: 'Change Percentage', value: `${company.changePercentage}%` },
    { title: 'Industry', value: company.industry },
    { title: 'CEO', value: company.ceo },
  ];
}

function profileEmptyMessage(ticker: string) {
  return `No company profile found for ${ticker.toUpperCase()}.`;
}

export default CompanyPage;
