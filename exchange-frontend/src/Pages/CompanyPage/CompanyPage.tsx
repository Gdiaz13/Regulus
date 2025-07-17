import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom';
import type { ICompanyProfile } from '../../Interfaces/APIResponses/ICompanyProfile';
import { getCompanyProfile } from '../../API/GET/getCompanyProfile';
import Sidebar from '../../Components/Sidebar/Sidebar';
import Dashboard from '../../Components/Dashboard/Dashboard';
import DashboardCard from '../../Components/Dashboard/DashboardCard';
import { formatCurrency } from '../../lib/formatters';

const CompanyPage = () => {
  let { ticker } = useParams();
  const [company, setCompany] = useState<ICompanyProfile>();

  useEffect(() => {
    const getProfileInfo = async () => {
      try {
        const result = await getCompanyProfile(ticker!);
        console.log('Company profile API result:', result);
        setCompany(result?.data[0]);
      } catch (error) {
        console.log('Error fetching company profile:', error);
      }
    };
    getProfileInfo();
    }, []);

  return (
    <div>
      {company && (
        <div>
          <Sidebar />
          <Dashboard ticker={ticker!}>
            <DashboardCard title='Company Name' value={company.companyName} />
            <DashboardCard title='Company Symbol' value={company.symbol} />
            <DashboardCard title='Price' value={formatCurrency(company.price)} />
            <DashboardCard title='Market Cap' value={formatCurrency(company.marketCap)} />
            <DashboardCard title='Change Percentage' value={`${company.changePercentage}%`} />
            <DashboardCard title='Industry' value={company.industry} />
            <DashboardCard title='CEO' value={company.ceo} />
            
            
           
          </Dashboard>
        </div>
      )}
    </div>
  );
};

export default CompanyPage;