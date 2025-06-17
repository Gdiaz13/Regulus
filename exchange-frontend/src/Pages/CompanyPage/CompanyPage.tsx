import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom';
import type { ICompanyProfile } from '../../Interfaces/APIResponses/ICompanyProfile';
import { getCompanyProfile } from '../../API/GET/getCompanyProfile';
import Sidebar from '../../Components/Sidebar/Sidebar';
import Dashboard from '../../Components/Dashboard/Dashboard';
import DashboardCard from '../../Components/Dashboard/DashboardCard';

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
      <Sidebar />
      <Dashboard ticker={ticker!}>
        {company && (
          <DashboardCard title='Company Name' value={company.companyName} />
        )}
      </Dashboard>
     
    </div>
  );
};

export default CompanyPage;