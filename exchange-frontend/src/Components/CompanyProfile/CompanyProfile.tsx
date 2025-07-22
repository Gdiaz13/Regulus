import { useEffect, useState } from 'react'
import type { ICompanyKeyMetrics } from '../../Interfaces/APIResponses/ICompanyKeyMetrics'
import { tableConfig } from './Config/CompanyProfileConfig'
import { useOutletContext } from 'react-router-dom';
import { getKeyMetrics } from '../../API/GET/getKeyMetrics';
import RatioList from '../RatioList/RatioList';
import Spinner from '../Spinner/Spinner';

const CompanyProfile = () => {
  const ticker = useOutletContext<string>();
  const [companyData, setCompanyData] = useState<ICompanyKeyMetrics | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const getCompanyKeyMetrics = async () => {
      try {
        const value = await getKeyMetrics(ticker);
        if (value?.data && value.data.length > 0) {
          setCompanyData(value.data[0]);
          setError(null);
        } else {
          setCompanyData(null);
          setError('No data found for this ticker.');
        }
      } catch (err) {
        setCompanyData(null);
        setError('Error fetching data.');
      }
    };
    getCompanyKeyMetrics();
  }, [ticker]);

  return (
    <>
      {companyData ? (
        <RatioList data={companyData} config={tableConfig} />
      ) : error ? (
        <div style={{textAlign: 'center', color: '#FFD700', marginTop: '2rem'}}>{error}</div>
      ) : (
        <Spinner />
      )}
    </>
  );
};

export default CompanyProfile;